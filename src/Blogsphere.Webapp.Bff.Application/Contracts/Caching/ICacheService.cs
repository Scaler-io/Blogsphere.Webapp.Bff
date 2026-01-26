using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Contracts.Caching
{
    public interface ICacheService
    {
        CacheServiceType Type { get; }
        Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, int? expirationTime = null, CancellationToken cancellation = default);
        Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync<T>(string key, T data, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}