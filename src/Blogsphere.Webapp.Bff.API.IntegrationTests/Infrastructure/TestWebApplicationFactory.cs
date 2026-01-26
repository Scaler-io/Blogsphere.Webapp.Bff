using Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure.Fakes;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure
{
    public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                // Ensure required options exist for service registration.
                var settings = new Dictionary<string, string?>
                {
                    ["ApiDescription"] = "Integration Tests",
                    ["ApiOriginHost"] = "localhost",

                    ["AppConfigurations:ApplicationIdentifier"] = "Blogsphere.Webapp.Bff.Api.Tests",
                    ["AppConfigurations:ApplicationEnvironment"] = "Testing",

                    ["IdentityGroupAccess:Authority"] = "http://localhost:5000",
                    ["IdentityGroupAccess:Audience"] = "blogsphere.webapp.bff.api",

                    ["Redis:InstanceName"] = "Blogsphere.Webapp.Bff.Api.Tests",
                    ["ConnectionString:Redis"] = "localhost:6379",

                    ["Jaeger:Host"] = "localhost",
                    ["Jaeger:Port"] = "6831",

                    // Provider settings (used to configure named HttpClients).
                    ["ProviderSettings:UserApiSettings:BaseUrl"] = "http://localhost:8000/user/",
                    ["ProviderSettings:UserApiSettings:ClientId"] = "test",
                    ["ProviderSettings:UserApiSettings:ClientSecret"] = "test",
                    ["ProviderSettings:UserApiSettings:SubscriptionKey"] = "test",

                    ["ProviderSettings:ApiGatewaySettings:BaseUrl"] = "http://localhost:8000/api/v1/",
                    ["ProviderSettings:ApiGatewaySettings:ClientId"] = "test",
                    ["ProviderSettings:ApiGatewaySettings:ClientSecret"] = "test",
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                // Override default auth to avoid real JWT validation.
                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                services.PostConfigureAll<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                });

                // Make health checks deterministic (avoid Redis dependency in tests).
                services.AddSingleton<AlwaysHealthyCheck>();
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    options.Registrations.Clear();
                    options.Registrations.Add(new HealthCheckRegistration(
                        "always-healthy",
                        sp => sp.GetRequiredService<AlwaysHealthyCheck>(),
                        HealthStatus.Unhealthy,
                        tags: null));
                });

                // Replace downstream providers with in-memory fakes.
                services.AddTransient<IApiGatewayProvider, FakeApiGatewayProvider>();
                services.AddTransient<IUserApiProvider, FakeUserApiProvider>();
            });
        }
    }
}
