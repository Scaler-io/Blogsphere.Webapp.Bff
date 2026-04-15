using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Analytics;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Scopes;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using FluentAssertions;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Features.Dashboard;

public class UserManagementDashboardScopeHandlerTests
{
    [Fact]
    public async Task HandleAsync_returns_cached_dashboard_without_calling_contributors()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var contributor = new Mock<IUserManagementDashboardAnalyticsContributor>(MockBehavior.Strict);
        var cache = new Mock<ICacheService>();
        var cacheFactory = new Mock<ICacheServiceFactory>();
        cacheFactory.Setup(f => f.GetCacheService(CacheServiceType.Distributed)).Returns(cache.Object);

        var cached = new UserManagementDashboardDto { Timestamp = DateTimeOffset.UtcNow };
        cache
            .Setup(c => c.GetAsync<UserManagementDashboardDto>(
                $"dashboard:{DashboardScopes.UserManagement}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var handler = new UserManagementDashboardScopeHandler(logger.Object, [contributor.Object], cacheFactory.Object);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), DashboardScopes.UserManagement);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(cached);
        contributor.Verify(
            c => c.ContributeAsync(It.IsAny<UserManagementDashboardAnalyticsDto>(), It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
        cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<UserManagementDashboardDto>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_a_contributor_fails()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var failing = new Mock<IUserManagementDashboardAnalyticsContributor>();
        failing
            .Setup(c => c.ContributeAsync(It.IsAny<UserManagementDashboardAnalyticsDto>(), It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(ErrorCode.OperationFailed, "search failed"));

        var cache = new Mock<ICacheService>();
        var cacheFactory = new Mock<ICacheServiceFactory>();
        cacheFactory.Setup(f => f.GetCacheService(CacheServiceType.Distributed)).Returns(cache.Object);
        cache
            .Setup(c => c.GetAsync<UserManagementDashboardDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<UserManagementDashboardDto>(null!));

        var handler = new UserManagementDashboardScopeHandler(logger.Object, [failing.Object], cacheFactory.Object);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), DashboardScopes.UserManagement);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.OperationFailed);
        result.ErrorMessage.Should().Be("search failed");
        cache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<UserManagementDashboardDto>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_on_cache_miss_runs_contributors_and_caches_result()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var contributor = new Mock<IUserManagementDashboardAnalyticsContributor>();
        contributor
            .Setup(c => c.ContributeAsync(It.IsAny<UserManagementDashboardAnalyticsDto>(), It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var cache = new Mock<ICacheService>();
        var cacheFactory = new Mock<ICacheServiceFactory>();
        cacheFactory.Setup(f => f.GetCacheService(CacheServiceType.Distributed)).Returns(cache.Object);
        cache
            .Setup(c => c.GetAsync<UserManagementDashboardDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<UserManagementDashboardDto>(null!));

        var handler = new UserManagementDashboardScopeHandler(logger.Object, [contributor.Object], cacheFactory.Object);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), DashboardScopes.UserManagement);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeOfType<UserManagementDashboardDto>();
        result.Data!.Kind.Should().Be(DashboardScopes.UserManagement);
        contributor.Verify(
            c => c.ContributeAsync(It.IsAny<UserManagementDashboardAnalyticsDto>(), request, It.IsAny<CancellationToken>()),
            Times.Once);
        cache.Verify(
            c => c.SetAsync(
                $"dashboard:{DashboardScopes.UserManagement}",
                It.Is<UserManagementDashboardDto>(d => d.Analytics != null),
                120,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
