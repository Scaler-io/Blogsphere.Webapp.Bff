using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.Swagger.Example.ApiGateway
{
    public class ApiClusterResponseExample : IExamplesProvider<ApiClusterDto>
    {
        public ApiClusterDto GetExamples() => new()
        {
            Id = "0c15927e-4453-47d6-b0ab-04658351c4e8",
            ClusterId = "usersvc",
            LoadBalancingPolicy = "RoundRobin",
            HealthCheckEnabled = "false",
            HealthCheckPath = "/health",
            HealthCheckInterval = "30",
            HealthCheckTimeout = "10",
            IsActive = true,
            Destinations =
            [
                new DestinationDto
                {
                    Id = "0e27d34f-58e3-4ae0-ae13-d08836595682",
                    DestinationId = "userapi-local",
                    Address = "http://localhost:5001",
                    IsActive = false
                },
                new DestinationDto
                {
                    Id = "8ef3ef17-0f52-45af-8d07-b3588a23ea05",
                    DestinationId = "userapi-docker",
                    Address = "http://localhost:8001",
                    IsActive = true
                }
            ],
            Routes = [
                new ClusterRouteDto
                {
                    Id = "02ef48a8-6eb4-4cde-a614-6b33f09f9bfb",
                    RouteId = "userApiHealthcheck",
                    Path = "/user/healthcheck",
                    Methods = [ "GET" ],
                    IsActive = true
                },
                new ClusterRouteDto
                {
                    Id = "317b77df-efb9-40b4-9dd2-d85644126957",
                    RouteId = "userApiWildcard",
                    Path = "/user/{**catch-all}",
                    Methods = [ "GET", "POST", "PUT", "DELETE" ],
                    IsActive = true
                }
            ],
            MetaData = new MetaDataDto
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = new OperationaUserDetailsDto
                {
                    EmployeeId = "ITADM2507188ED9",
                    FullName = "Super Admin",
                    Email = "superadmin@blogsphere.com",
                    JobTitle = "System Administrator"
                },
                UpdatedBy = new OperationaUserDetailsDto
                {
                    EmployeeId = "ITADM2507188ED9",
                    FullName = "Super Admin",
                    Email = "superadmin@blogsphere.com",
                    JobTitle = "System Administrator"
                }
            }
        };
    }
}
