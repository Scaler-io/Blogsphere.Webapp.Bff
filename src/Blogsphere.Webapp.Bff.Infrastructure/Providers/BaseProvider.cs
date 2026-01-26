using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using IdentityModel;

namespace Blogsphere.Webapp.Bff.Infrastructure.Providers
{
    public class BaseProvider(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        ITokenExchangeService tokenExchangeService,
        ICacheService cacheService,
        string apiProviderName)
    {
        private readonly ILogger _logger = logger;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ITokenExchangeService _tokenExchangeService = tokenExchangeService;
        protected readonly ICacheService _cacheService = cacheService;


        protected async Task<HttpClient> GetHttpClientAsync(RequestInformation requestInformation, string scope, bool isPublic = false)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(apiProviderName);
                if (client is null)
                {
                    _logger.Here().Error("Failed to create HTTP client for provider: {ProviderName}", apiProviderName);
                    throw new InvalidOperationException($"HTTP client '{apiProviderName}' was not configured");
                }

                if (!isPublic)
                {
                    _logger.Here().Information("Getting access token on behalf of provider: {ProviderName}", apiProviderName);
                    var (userId, tokenExpiry) = ParseTokenClaims(requestInformation.CurrentUser.Authorization.Token);
                    var safeExpiry = CalculateSafeExpiry(tokenExpiry);
                    var cacheKey = $"obo_token:{userId}:{scope}:{apiProviderName}";

                    if (await _cacheService.ContainsAsync(cacheKey))
                    {
                        _logger.Here().Information("Using cached token for provider: {ProviderName}", apiProviderName);
                        var cachedToken = await _cacheService.GetAsync<string>(cacheKey);
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cachedToken);
                        return client;
                    }

                    _logger.Here().Information("Exchanging token for provider: {ProviderName}", apiProviderName);
                    var token = await _tokenExchangeService.ExchangeTokenAsync(requestInformation.CurrentUser.Authorization.Token, scope);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Calculate TTL (time-to-live) in seconds from now
                    var ttlSeconds = (int)(safeExpiry - DateTimeOffset.UtcNow).TotalSeconds;
                    _logger.Here().Information("Caching token for {TTL} seconds (expires at {SafeExpiry} UTC)", ttlSeconds, safeExpiry);
                    await _cacheService.SetAsync(cacheKey, token, ttlSeconds);
                }

                return client;
            }
            catch (Exception ex)
            {
                _logger.Here().Error(ex, "Error creating HTTP client for provider: {ProviderName}", apiProviderName);
                throw;
            }
        }

        private DateTimeOffset CalculateSafeExpiry(DateTimeOffset tokenExpiry)
        {
            // Add a safety buffer (e.g., 2-5 minutes before actual expiry)
            var safetyBuffer = TimeSpan.FromMinutes(2);
            var bufferredExpiry = tokenExpiry.Subtract(safetyBuffer);

            // Also cap at 55 minutes from now (in case the original token is very long-lived)
            var maxCacheDuration = DateTimeOffset.UtcNow.AddMinutes(55);

            // Return the EARLIER of the two
            return bufferredExpiry < maxCacheDuration ? bufferredExpiry : maxCacheDuration;
        }

        private (object userId, DateTimeOffset tokenExpiry) ParseTokenClaims(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject)?.Value
                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value
                    ?? throw new InvalidOperationException("User ID not found in token");

                // JWT ValidTo is in UTC, ensure it's explicitly treated as UTC
                var expiry = new DateTimeOffset(jwtToken.ValidTo, TimeSpan.Zero);

                _logger.Here().Information("Token expires at: {Expiry} (UTC)", expiry);
                return (userId, expiry);
            }
            catch (Exception ex)
            {
                _logger.Here().Error(ex, "Error parsing token claims: {Exception}", ex.Message);
                throw;
            }
        }
    }
}