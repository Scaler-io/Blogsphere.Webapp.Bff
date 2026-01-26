using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Blogsphere.Webapp.Bff.Infrastructure.HealthChecks
{
    public class RedisHealthCheck(IDistributedCache distributedCache) : IHealthCheck
    {
        private readonly IDistributedCache _distributedCache = distributedCache;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _distributedCache.SetStringAsync("redis_health", "OK", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1),
                }, cancellationToken);

                return HealthCheckResult.Healthy("Redis health check is a success");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Redis health check failed", ex);
            }
        }
    }
}