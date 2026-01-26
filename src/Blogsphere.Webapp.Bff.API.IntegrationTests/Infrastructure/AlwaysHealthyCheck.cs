using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure
{
    public sealed class AlwaysHealthyCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Test health check"));
        }
    }
}
