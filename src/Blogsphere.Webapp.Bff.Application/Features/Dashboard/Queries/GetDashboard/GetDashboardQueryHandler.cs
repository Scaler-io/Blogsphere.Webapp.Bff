using System.Globalization;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQueryHandler(
        ILogger logger,
        ISearchApiProvider searchApiProvider,
        IApiGatewayProvider apiGatewayProvider,
        ICacheServiceFactory cacheServiceFactory) : IQueryHandler<GetDashboardQuery, Result<DashboardDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly ISearchApiProvider _searchApiProvider = searchApiProvider;
        private readonly IApiGatewayProvider _apiGatewayProvider = apiGatewayProvider;
        private readonly ICacheService _cacheService = cacheServiceFactory.GetCacheService(CacheServiceType.Distributed);

        public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            _logger.Here().MethodEntered();

            var cacheKey = "dashboard";
            var cachedDashboard = await _cacheService.GetAsync<DashboardDto>(cacheKey, cancellationToken);
            if (cachedDashboard != null)
            {
                return Result<DashboardDto>.Success(cachedDashboard);
            }

            var nowUtc = DateTimeOffset.UtcNow;
            var monthBuckets = GetLastSixMonthBuckets(nowUtc);
            var monthLabels = monthBuckets.Select(x => x.Label).ToArray();

            var rangeStartUtc = monthBuckets[0].StartUtc;
            var rangeEndUtcExclusive = monthBuckets[^1].EndExclusiveUtc;

            var lastSixMonthsSearchRequest = new PaginatedSearchRequest
            {
                SearchType = SearchType.All,
                PageIndex = 1,
                PageSize = 1,
                SortField = "createdAt",
                SortOrder = "desc",
                IsFilteredQuery = true,
                StartTime = rangeStartUtc.UtcDateTime,
                EndTime = rangeEndUtcExclusive.UtcDateTime,
                TimeField = "createdAt",
            };

            var clustersTask = _searchApiProvider.SearchClustersAsync(lastSixMonthsSearchRequest, request.RequestInformation, cancellationToken);
            var routesTask = _searchApiProvider.SearchRoutesAsync(lastSixMonthsSearchRequest, request.RequestInformation, cancellationToken);
            var totalClustersTask = _searchApiProvider.GetTotalClustersCountAsync(request.RequestInformation, cancellationToken);
            var totalRoutesTask = _searchApiProvider.GetTotalRoutesCountAsync(request.RequestInformation, cancellationToken);
            var apiProductsTask = _apiGatewayProvider.GetApiProductsAsync(request.RequestInformation, cancellationToken);

            await Task.WhenAll(clustersTask, routesTask, totalClustersTask, totalRoutesTask, apiProductsTask);

            var clustersResult = await clustersTask;
            if (!clustersResult.IsSuccess)
            {
                return Result<DashboardDto>.Failure(clustersResult.ErrorCode, clustersResult.ErrorMessage);
            }

            var routesResult = await routesTask;
            if (!routesResult.IsSuccess)
            {
                return Result<DashboardDto>.Failure(routesResult.ErrorCode, routesResult.ErrorMessage);
            }

            var clusters = clustersResult.Data?.Data ?? [];
            var routes = routesResult.Data?.Data ?? [];

            var totalClustersResult = await totalClustersTask;
            if (!totalClustersResult.IsSuccess)
            {
                return Result<DashboardDto>.Failure(totalClustersResult.ErrorCode, totalClustersResult.ErrorMessage);
            }

            var totalRoutesResult = await totalRoutesTask;
            if (!totalRoutesResult.IsSuccess)
            {
                return Result<DashboardDto>.Failure(totalRoutesResult.ErrorCode, totalRoutesResult.ErrorMessage);
            }

            var apiProductsResult = await apiProductsTask;
            if (!apiProductsResult.IsSuccess)
            {
                return Result<DashboardDto>.Failure(apiProductsResult.ErrorCode, apiProductsResult.ErrorMessage);
            }

            var clusterCreatedAt = clusters.Select(x => ToUtc(x.CreatedAt)).ToArray();
            var routeCreatedAt = routes.Select(x => ToUtc(x.CreatedAt)).ToArray();

            var clusterGrowth = monthBuckets
                .Select(bucket => clusterCreatedAt.LongCount(d => d < bucket.EndExclusiveUtc))
                .ToArray();

            var routeGrowth = monthBuckets
                .Select(bucket => routeCreatedAt.LongCount(d => d < bucket.EndExclusiveUtc))
                .ToArray();

            var monthlyRouteActivity = monthBuckets
                .Select(bucket => routeCreatedAt.LongCount(d => d >= bucket.StartUtc && d < bucket.EndExclusiveUtc))
                .ToArray();

            var activeClusters = clusters.LongCount(x => IsActiveStatus(x.Status));
            var inactiveClusters = clusters.LongCount() - activeClusters;

            var productsCount = apiProductsResult.Data.TotalCount;

            var dashboard = new DashboardDto
            {
                Summary = new DashboardSummaryDto
                {
                    Clusters = totalClustersResult.Data,
                    Routes = totalRoutesResult.Data,
                    Products = productsCount,
                },
                Charts = new DashboardChartsDto
                {
                    GrowthTrend = new DashboardGrowthTrendChartDto
                    {
                        Labels = monthLabels,
                        Datasets =
                        [
                            new DashboardDatasetDto { Label = "Clusters", Data = clusterGrowth },
                            new DashboardDatasetDto { Label = "Routes", Data = routeGrowth },
                        ]
                    },
                    ClusterStatusDistribution = new DashboardStatusDistributionChartDto
                    {
                        Labels = ["Active", "Inactive"],
                        Counts = [activeClusters, inactiveClusters],
                    },
                    MonthlyRouteActivity = new DashboardMonthlyCountsChartDto
                    {
                        Labels = monthLabels,
                        Counts = monthlyRouteActivity,
                    }
                },
                Timestamp = nowUtc,
                RefreshAfterSeconds = 120,
            };

            await _cacheService.SetAsync("dashboard", dashboard, 120, cancellationToken);

            _logger.Here().MethodExited();
            return Result<DashboardDto>.Success(dashboard);
        }

        private static bool IsActiveStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            var normalized = status.Trim();

            if (normalized.Equals("inactive", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("disabled", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("offline", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("down", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("stopped", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Treat anything else ("active", "healthy", "degraded", etc.) as active.
            return true;
        }

        private static DateTimeOffset ToUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return new DateTimeOffset(dateTime);
            }

            if (dateTime.Kind == DateTimeKind.Local)
            {
                return new DateTimeOffset(dateTime).ToUniversalTime();
            }

            return new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        }

        private static IReadOnlyList<MonthBucket> GetLastSixMonthBuckets(DateTimeOffset nowUtc)
        {
            var thisMonthStartUtc = new DateTimeOffset(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var startUtc = thisMonthStartUtc.AddMonths(-5);

            var buckets = new List<MonthBucket>(capacity: 6);
            for (var i = 0; i < 6; i++)
            {
                var monthStartUtc = startUtc.AddMonths(i);
                var monthEndExclusiveUtc = monthStartUtc.AddMonths(1);
                buckets.Add(new MonthBucket(
                    monthStartUtc.ToString("MMM", CultureInfo.InvariantCulture),
                    monthStartUtc,
                    monthEndExclusiveUtc));
            }

            return buckets;
        }

        private sealed record MonthBucket(string Label, DateTimeOffset StartUtc, DateTimeOffset EndExclusiveUtc);
    }
}

