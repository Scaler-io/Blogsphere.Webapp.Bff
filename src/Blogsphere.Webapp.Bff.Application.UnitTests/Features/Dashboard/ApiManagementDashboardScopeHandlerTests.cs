using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Scopes;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Entities.Search;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using FluentAssertions;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Features.Dashboard;

public class ApiManagementDashboardScopeHandlerTests
{
    [Fact]
    public async Task HandleAsync_returns_cached_dashboard_without_calling_providers()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var searchApi = new Mock<ISearchApiProvider>(MockBehavior.Strict);
        var apiGateway = new Mock<IApiGatewayProvider>(MockBehavior.Strict);
        var cache = new Mock<ICacheService>();
        var cacheFactory = new Mock<ICacheServiceFactory>();
        cacheFactory.Setup(f => f.GetCacheService(CacheServiceType.Distributed)).Returns(cache.Object);

        var cached = new ApiManagementDashboardDto { Timestamp = DateTimeOffset.UtcNow };
        cache
            .Setup(c => c.GetAsync<ApiManagementDashboardDto>(
                $"dashboard:{DashboardScopes.ApiManagement}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var handler = new ApiManagementDashboardScopeHandler(logger.Object, searchApi.Object, apiGateway.Object, cacheFactory.Object);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), DashboardScopes.ApiManagement);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(cached);
    }

    [Fact]
    public async Task HandleAsync_returns_failure_when_cluster_search_fails()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var searchApi = new Mock<ISearchApiProvider>();
        searchApi
            .Setup(s => s.SearchClustersAsync(It.IsAny<PaginatedSearchRequest>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<ApiClusterSummary>>.Failure(ErrorCode.OperationFailed, "cluster search error"));
        WireSuccessfulRouteSearchAndTotals(searchApi);

        var apiGateway = new Mock<IApiGatewayProvider>();
        apiGateway
            .Setup(g => g.GetApiProductsAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ApiProductListDto>.Success(new ApiProductListDto { TotalCount = 0, ApiProducts = [] }));

        var cache = CreateCacheMiss();
        var cacheFactory = CreateCacheFactory(cache.Object);

        var handler = new ApiManagementDashboardScopeHandler(logger.Object, searchApi.Object, apiGateway.Object, cacheFactory.Object);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), DashboardScopes.ApiManagement);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.OperationFailed);
        result.ErrorMessage.Should().Be("cluster search error");
    }

    [Fact]
    public async Task HandleAsync_on_cache_miss_aggregates_search_and_products_into_dashboard()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var searchApi = new Mock<ISearchApiProvider>();
        var emptyClusters = new PaginatedResult<ApiClusterSummary>(1, 1, 0, []);
        var emptyRoutes = new PaginatedResult<ApiRouteSummary>(1, 1, 0, []);
        searchApi
            .Setup(s => s.SearchClustersAsync(It.IsAny<PaginatedSearchRequest>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<ApiClusterSummary>>.Success(emptyClusters));
        searchApi
            .Setup(s => s.SearchRoutesAsync(It.IsAny<PaginatedSearchRequest>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<ApiRouteSummary>>.Success(emptyRoutes));
        searchApi
            .Setup(s => s.GetTotalClustersCountAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Success(7));
        searchApi
            .Setup(s => s.GetTotalRoutesCountAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Success(42));

        var apiGateway = new Mock<IApiGatewayProvider>();
        apiGateway
            .Setup(g => g.GetApiProductsAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ApiProductListDto>.Success(new ApiProductListDto { TotalCount = 5, ApiProducts = [] }));

        var cache = CreateCacheMiss();
        var cacheFactory = CreateCacheFactory(cache.Object);

        var handler = new ApiManagementDashboardScopeHandler(logger.Object, searchApi.Object, apiGateway.Object, cacheFactory.Object);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), DashboardScopes.ApiManagement);

        var result = await handler.HandleAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Data.Should().BeOfType<ApiManagementDashboardDto>().Subject;
        dto.Kind.Should().Be(DashboardScopes.ApiManagement);
        dto.Summary.Clusters.Should().Be(7);
        dto.Summary.Routes.Should().Be(42);
        dto.Summary.Products.Should().Be(5);
        dto.Charts.Should().NotBeNull();
        dto.Charts.GrowthTrend.Labels.Should().HaveCount(6);
        cache.Verify(
            c => c.SetAsync(
                $"dashboard:{DashboardScopes.ApiManagement}",
                It.IsAny<ApiManagementDashboardDto>(),
                120,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static void WireSuccessfulRouteSearchAndTotals(Mock<ISearchApiProvider> searchApi)
    {
        var emptyRoutes = new PaginatedResult<ApiRouteSummary>(1, 1, 0, []);
        searchApi
            .Setup(s => s.SearchRoutesAsync(It.IsAny<PaginatedSearchRequest>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<ApiRouteSummary>>.Success(emptyRoutes));
        searchApi
            .Setup(s => s.GetTotalClustersCountAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Success(0));
        searchApi
            .Setup(s => s.GetTotalRoutesCountAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Success(0));
    }

    private static Mock<ICacheService> CreateCacheMiss()
    {
        var cache = new Mock<ICacheService>();
        cache
            .Setup(c => c.GetAsync<ApiManagementDashboardDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<ApiManagementDashboardDto>(null!));
        return cache;
    }

    private static Mock<ICacheServiceFactory> CreateCacheFactory(ICacheService cache)
    {
        var cacheFactory = new Mock<ICacheServiceFactory>();
        cacheFactory.Setup(f => f.GetCacheService(CacheServiceType.Distributed)).Returns(cache);
        return cacheFactory;
    }
}
