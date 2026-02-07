using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.Swagger.Example.ApiGateway
{
    public class ApiRouteResponseExample : IExamplesProvider<ApiRouteDto>
    {
        public ApiRouteDto GetExamples() => new()
        {
            Id = "0c15927e-4453-47d6-b0ab-04658351c4e8",
            RouteId = "userApiHealthcheck",
            Path = "/user/healthcheck",
            Methods = ["GET"],
            IsActive = true,
            ClusterDetails = new BasicApiClusterDetailsDto
            {
                Id = "usersvc",
                ClusterId = "usersvc"
            },
            Headers = [
                new ApiRouteHeaderDto
                {
                    Id = "0c15927e-4453-47d6-b0ab-04658351c4e8",
                    Name = "api-version",
                    Values = ["v1"],
                    Mode = "Exact",
                    IsActive = true
                }
            ],
            Transforms = [
                new ApiRouteTransformDto
                {
                    Id = "0c15927e-4453-47d6-b0ab-04658351c4e8",
                    PathPattern = "/user/healthcheck",
                    IsActive = true
                }
            ],
            Metadata = new MetaDataDto
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
