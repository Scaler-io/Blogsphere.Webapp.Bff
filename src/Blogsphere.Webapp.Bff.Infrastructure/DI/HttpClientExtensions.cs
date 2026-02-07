using System.Net.Http.Headers;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange;
using Blogsphere.Webapp.Bff.Domain.Configurations;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Infrastructure.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Blogsphere.Webapp.Bff.Infrastructure.DI
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            var providerConfiguration = configuration.GetSection(ProviderConfigurationOption.OptionName)
                .Get<ProviderConfigurationOption>();

            var idetityAuthority = configuration["IdentityGroupAccess:Authority"];

            services.AddHttpClient(ApiProviderNames.IdentityServer, client =>
            {
                client.BaseAddress = new Uri(idetityAuthority);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            services.AddHttpClient(ApiProviderNames.UserApi, client =>
            {
                client.BaseAddress = new Uri(providerConfiguration.UserApiSettings.BaseUrl);
                client.DefaultRequestHeaders.Add("ocp-apim-subscriptionkey", providerConfiguration.UserApiSettings.SubscriptionKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient(ApiProviderNames.ApiGateway, client =>
            {
                client.BaseAddress = new Uri(providerConfiguration.ApiGatewaySettings.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient(ApiProviderNames.SearchApi, client =>
            {
                client.BaseAddress = new Uri(providerConfiguration.SearchApiSettings.BaseUrl);
                client.DefaultRequestHeaders.Add("ocp-apim-subscriptionkey", providerConfiguration.SearchApiSettings.SubscriptionKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddTransient<IUserApiProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var tokenExchangeService = sp.GetRequiredService<ITokenExchangeService>();
                var cacheServiceFactory = sp.GetRequiredService<ICacheServiceFactory>();
                var jsonSerializerSettings = sp.GetRequiredService<JsonSerializerSettings>();
                return new UserApiProvider(logger, httpClientFactory, tokenExchangeService, cacheServiceFactory, jsonSerializerSettings, ApiProviderNames.UserApi);
            });

            services.AddTransient<IApiGatewayProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var tokenExchangeService = sp.GetRequiredService<ITokenExchangeService>();
                var cacheServiceFactory = sp.GetRequiredService<ICacheServiceFactory>();
                return new ApiGatewayProvider(
                    logger,
                    httpClientFactory,
                    tokenExchangeService,
                    cacheServiceFactory,
                    ApiProviderNames.ApiGateway,
                    sp.GetRequiredService<JsonSerializerSettings>());
            });

            services.AddTransient<ISearchApiProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var tokenExchangeService = sp.GetRequiredService<ITokenExchangeService>();
                var cacheServiceFactory = sp.GetRequiredService<ICacheServiceFactory>();
                var jsonSerializerSettings = sp.GetRequiredService<JsonSerializerSettings>();
                return new SearchApiProvider(logger, httpClientFactory, tokenExchangeService, cacheServiceFactory, jsonSerializerSettings, ApiProviderNames.SearchApi);
            });

            return services;
        }
    }
}