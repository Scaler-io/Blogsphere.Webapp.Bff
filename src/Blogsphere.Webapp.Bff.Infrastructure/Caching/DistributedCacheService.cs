using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.ActivityTracker;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Blogsphere.Webapp.Bff.Domain.Configurations;
using Microsoft.Extensions.Options;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Newtonsoft.Json;

namespace Blogsphere.Webapp.Bff.Infrastructure.Caching
{
    public class DistributedCacheService(
        IActivityTracker activityTracker,
        IDistributedCache distributedCache,
        IOptions<AppConfigOption> appConfigOption) : ICacheService
    {
        private readonly IActivityTracker _activityTracker = activityTracker;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly AppConfigOption _appConfigOption = appConfigOption.Value;


        public CacheServiceType Type { get; } = CacheServiceType.Distributed;

        public async Task<T> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            using var activity = _activityTracker.TrackRedisActivity(nameof(GetAsync), cacheKey);
            var data = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (data is null)
            {
                activity?.SetTag(TrackerConstants.CacheHit, false);
                return default;
            }
            activity?.SetTag(TrackerConstants.CacheHit, true);
            return JsonConvert.DeserializeObject<T>(data);
        }
        public async Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default)
        {
            using var activity = _activityTracker.TrackRedisActivity("CONTAINS", key);
            return (await _distributedCache.GetStringAsync(key, cancellationToken)) is not null;
        }

        public async Task SetAsync<T>(string key, T value, int? expirationTime = null, CancellationToken cancellation = default)
        {
            using var activity = _activityTracker.TrackRedisActivity("SET", key);
            var serializedData = JsonConvert.SerializeObject(value);
            var cacheOptions = new DistributedCacheEntryOptions();

            // expirationTime is in seconds, convert to TimeSpan
            if (expirationTime.HasValue)
            {
                cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(expirationTime.Value));
            }
            else
            {
                cacheOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(_appConfigOption.CacheExpiration));
            }

            await _distributedCache.SetStringAsync(key, serializedData, cacheOptions, cancellation);
        }
        public async Task<T> UpdateAsync<T>(string key, T data, CancellationToken cancellation = default)
        {
            using var activity = _activityTracker.TrackRedisActivity("UPDATE", key);
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(data), cancellation);
            return await GetAsync<T>(key, cancellation);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            using var activity = _activityTracker.TrackRedisActivity("REMOVE", key);
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
    }
}