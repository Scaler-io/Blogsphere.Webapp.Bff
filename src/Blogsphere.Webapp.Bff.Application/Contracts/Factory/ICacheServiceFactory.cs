using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Contracts.Factory
{
    public interface ICacheServiceFactory
    {
        ICacheService GetCacheService(CacheServiceType cacheServiceType);
    }
}