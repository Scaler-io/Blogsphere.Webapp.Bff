using System.Diagnostics;
using Blogsphere.Webapp.Bff.Application.Contracts.ActivityTracker;
using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Contracts.Security;
using Blogsphere.Webapp.Bff.Application.Contracts.TokenExchange;
using Blogsphere.Webapp.Bff.Infrastructure.Caching;
using Blogsphere.Webapp.Bff.Infrastructure.Factory;
using Blogsphere.Webapp.Bff.Infrastructure.HealthChecks;
using Blogsphere.Webapp.Bff.Infrastructure.Security;
using Blogsphere.Webapp.Bff.Infrastructure.TokenExchange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blogsphere.Webapp.Bff.Infrastructure.DI
{
    public static class InfrastructureServicesExtension
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IIdentityService, IdentityService>();

            // health checks
            services.AddHealthChecks()
            .AddCheck<RedisHealthCheck>("redis-health");

            // cache service
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = configuration["Redis:InstanceName"];
                options.Configuration = configuration["ConnectionString:Redis"];
            });

            // activity tracker
            services.AddSingleton(new ActivitySource("Blogsphere.Webapp.Bff.Api"));
            services.AddSingleton<IActivityTracker, ActivityTracker.ActivityTracker>();

            // cache service
            services.AddScoped<DistributedCacheService>();
            services.AddScoped<ICacheServiceFactory, CacheServiceFactory>();

            // http client factory
            services.AddHttpClient();

            // token exchange service
            services.AddScoped<ITokenExchangeService, TokenExchangeService>();

            return services;
        }
    }
}