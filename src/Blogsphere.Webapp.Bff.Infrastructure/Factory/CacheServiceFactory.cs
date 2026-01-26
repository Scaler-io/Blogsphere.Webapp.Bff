using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using Blogsphere.Webapp.Bff.Infrastructure.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace Blogsphere.Webapp.Bff.Infrastructure.Factory
{
    public class CacheServiceFactory(IServiceProvider serviceProvider) : ICacheServiceFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public ICacheService GetCacheService(CacheServiceType cacheServiceType)
        {
            return cacheServiceType switch
            {
                // CacheServiceType.InMemory => _serviceProvider.GetRequiredService<InMemoryCacheService>(),
                CacheServiceType.Distributed => _serviceProvider.GetRequiredService<DistributedCacheService>(),
                _ => throw new ArgumentException("Invalid cache service type")
            };
        }
    }
}