using Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Configurations;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Blogsphere.Webapp.Bff.Infrastructure.TokenExchange
{
    public class TokenExchangeService(
        IHttpClientFactory httpClientFactory,
        IOptions<ProviderConfigurationOption> providerConfigurationOption,
        IConfiguration configuration,
        ILogger logger) : ITokenExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ProviderConfigurationOption _providerConfigurationOption = providerConfigurationOption.Value;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger _logger = logger;

        public async Task<string> ExchangeTokenAsync(string originalToken, string scope)
        {
            var client = _httpClientFactory.CreateClient(ApiProviderNames.IdentityServer);
            var validateDiscoveryIssuerName = _configuration.GetValue("IdentityGroupAccess:ValidateDiscoveryIssuerName", true);
            var discoveryDocument = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false,
                    // In Docker/local dev, IdentityServer might use a fixed IssuerUri (e.g. http://localhost:5000)
                    // while the BFF reaches it via host.docker.internal. In that scenario, issuer != authority is expected.
                    ValidateIssuerName = validateDiscoveryIssuerName,
                    ValidateEndpoints = true
                }
            });

            if (discoveryDocument.IsError)
            {
                _logger.Here().Error("Failed to get discovery document from identity server: {Error}", discoveryDocument.Error);
                throw new HttpRequestException(discoveryDocument.Error);
            }

            var response = await client.RequestTokenAsync(new TokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                GrantType = "delegation",
                ClientId = "blogsphere.webapp.bff.api",
                ClientSecret = "bff-secret-key-2024",
                Parameters =
                {
                    {"scope", scope},
                    {"token", originalToken}
                },
            });

            if (response.IsError)
            {
                _logger.Here().Error("Error exchanging token: {Error}", response.Error);
                return string.Empty;
            }

            return response.AccessToken;
        }
    }
}