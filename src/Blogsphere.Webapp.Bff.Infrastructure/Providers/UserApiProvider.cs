using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using Newtonsoft.Json;

namespace Blogsphere.Webapp.Bff.Infrastructure.Providers
{
    public class UserApiProvider(
        ILogger logger,
        IHttpClientFactory httpClientFactory,
        ITokenExchangeService tokenExchangeService,
        ICacheServiceFactory cacheServiceFactory,
        JsonSerializerSettings jsonSerializerSettings,
        string apiProviderName) : BaseProvider(logger, httpClientFactory, tokenExchangeService, cacheServiceFactory.GetCacheService(CacheServiceType.Distributed), apiProviderName), IUserApiProvider
    {
        private const string CorrelationIdHeader = "CorrelationId";
        private const string ApiVersionHeader = "api-version";
        private const string ApiVersion = "v2";

        private readonly ILogger _logger = logger;
        private readonly JsonSerializerSettings _jsonSerializerSettings = jsonSerializerSettings;

        public async Task<Result<ManagementUserDetails>> GetManagementUserNameDetailsById(string userId, RequestInformation requestInformation, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<ManagementUserDetails>.Failure(ErrorCode.BadRequest, ErrorMessages.BadRequest);
            }

            var cacheKey = GetUserCacheKey(userId);
            var cachedUser = await TryGetCachedUserAsync(cacheKey, cancellationToken);
            if (cachedUser is not null)
            {
                return Result<ManagementUserDetails>.Success(cachedUser);
            }

            var userApiHttpClient = await GetHttpClientAsync(requestInformation, "userapi:read");
            SetUserApiHeaders(userApiHttpClient, requestInformation);

            var userResult = await FetchUserAsync(userApiHttpClient, userId, cancellationToken);
            if (!userResult.IsSuccess)
            {
                return userResult;
            }

            await TryCacheUserAsync(cacheKey, userResult.Data, cancellationToken);
            return userResult;
        }

        private static string GetUserCacheKey(string userId) => $"user_details:{userId}";

        private async Task<ManagementUserDetails> TryGetCachedUserAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                return await _cacheService.GetAsync<ManagementUserDetails>(cacheKey, cancellationToken);
            }
            catch (Exception ex)
            {
                // Cache failures shouldn't block the request.
                _logger.Here().Error(ex, "Failed to read user cache for key: {CacheKey}", cacheKey);
                return null;
            }
        }

        private async Task TryCacheUserAsync(string cacheKey, ManagementUserDetails user, CancellationToken cancellationToken)
        {
            try
            {
                // Uses default cache TTL (AppConfigurations:CacheExpiration minutes)
                await _cacheService.SetAsync(cacheKey, user, expirationTime: 1800, cancellation: cancellationToken);
            }
            catch (Exception ex)
            {
                // Cache failures shouldn't block the request.
                _logger.Here().Error(ex, "Failed to write user cache for key: {CacheKey}", cacheKey);
            }
        }

        private static void SetUserApiHeaders(HttpClient client, RequestInformation requestInformation)
        {
            if (client.DefaultRequestHeaders.Contains(CorrelationIdHeader))
            {
                client.DefaultRequestHeaders.Remove(CorrelationIdHeader);
            }
            client.DefaultRequestHeaders.Add(CorrelationIdHeader, requestInformation.CorreationId);

            if (client.DefaultRequestHeaders.Contains(ApiVersionHeader))
            {
                client.DefaultRequestHeaders.Remove(ApiVersionHeader);
            }
            client.DefaultRequestHeaders.Add(ApiVersionHeader, ApiVersion);
        }

        private async Task<Result<ManagementUserDetails>> FetchUserAsync(HttpClient client, string userId, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync($"managementuser/{userId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Result<ManagementUserDetails>.Failure(ErrorCode.InternalServerError, ErrorMessages.Operationfailed);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var user = JsonConvert.DeserializeObject<ManagementUserDetails>(content, _jsonSerializerSettings);
            return user is null
                ? Result<ManagementUserDetails>.Failure(ErrorCode.InternalServerError, ErrorMessages.InternalServerError)
                : Result<ManagementUserDetails>.Success(user);
        }
    }
}