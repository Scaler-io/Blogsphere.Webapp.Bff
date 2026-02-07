using Blogsphere.Webapp.Bff.Domain.Entities;
using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Common;

public static class TestData
{
    private static readonly DateTime FixedUtcNow = new(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

    public static MetaData MetaData(
        string createdBy = "creator-id",
        string updatedBy = "updater-id",
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new MetaData
        {
            CreatedBy = createdBy,
            UpdatedBy = updatedBy,
            CreatedAt = createdAt ?? FixedUtcNow.AddDays(-1),
            UpdatedAt = updatedAt ?? FixedUtcNow,
        };
    }

    public static ApiRoute ApiRoute(
        string id = "route-1",
        string createdBy = "creator-id",
        string updatedBy = "updater-id")
    {
        return new ApiRoute
        {
            Id = id,
            RouteId = "r1",
            Path = "/route1",
            Methods = ["GET"],
            RateLimiterPolicy = "rate-limiter-policy",
            IsActive = true,
            ClusterId = "cluster-1",
            Headers = [],
            Transforms = [],
            Metadata = MetaData(createdBy: createdBy, updatedBy: updatedBy),
        };
    }

    public static ApiCluster ApiCluster(
        string id = "cluster-1",
        string createdBy = "creator-id",
        string updatedBy = "updater-id")
    {
        return new ApiCluster
        {
            Id = id,
            ClusterId = "c1",
            LoadBalancingPolicy = "RoundRobin",
            HealthCheckEnabled = "true",
            HealthCheckPath = "/health",
            HealthCheckInterval = "00:00:10",
            HealthCheckTimeout = "00:00:05",
            IsActive = true,
            Destinations = [],
            Routes = [],
            MetaData = MetaData(createdBy: createdBy, updatedBy: updatedBy),
        };
    }

    public static ManagementUserDetails ManagementUser(
        string id,
        string fullName,
        string email = "user@local",
        string employeeId = "E1",
        string jobTitle = "User")
    {
        return new ManagementUserDetails
        {
            Id = id,
            FullName = fullName,
            Email = email,
            EmployeeId = employeeId,
            JobTitle = jobTitle,
        };
    }
}

