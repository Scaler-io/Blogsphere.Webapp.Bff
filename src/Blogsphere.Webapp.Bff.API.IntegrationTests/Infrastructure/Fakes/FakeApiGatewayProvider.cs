using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Domain.Entities;
using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Models.Core;

namespace Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure.Fakes
{
    public sealed class FakeApiGatewayProvider : IApiGatewayProvider
    {
        public Task<Result<ApiCluster>> GetApiCLusterDetailsByIdAsync(
            string id,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var cluster = new ApiCluster
            {
                Id = id,
                ClusterId = "test-cluster",
                LoadBalancingPolicy = "RoundRobin",
                HealthCheckEnabled = "true",
                HealthCheckPath = "/health",
                HealthCheckInterval = "00:00:10",
                HealthCheckTimeout = "00:00:05",
                IsActive = true,
                Destinations = [],
                Routes = [],
                MetaData = new MetaData
                {
                    CreatedBy = "creator-id",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedBy = "updater-id",
                    UpdatedAt = DateTime.UtcNow,
                }
            };

            return Task.FromResult(Result<ApiCluster>.Success(cluster));
        }

        public Task<Result<ApiRoute>> GetApiRouteByIdAsync(string id, RequestInformation requestInformation, CancellationToken cancellationToken = default)
        {
            var route = new ApiRoute
            {
                Id = id,
                Path = "/test-route",
                Methods = ["GET", "POST", "PUT", "DELETE"],
                IsActive = true,
                ClusterId = "test-cluster",
            };

            return Task.FromResult(Result<ApiRoute>.Success(route));
        }
    }
}
