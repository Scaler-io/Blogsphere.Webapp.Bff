using System.Globalization;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Domain.Entities.Search;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard.Analytics
{
    public class ManagementUserDashboardAnalyticsContributor(ISearchApiProvider searchApiProvider) : IUserManagementDashboardAnalyticsContributor
    {
        private const int SearchPageSize = 500;
        private const int TopN = 8;

        private readonly ISearchApiProvider _searchApiProvider = searchApiProvider;

        public async Task<Result<bool>> ContributeAsync(
            UserManagementDashboardAnalyticsDto analytics,
            GetDashboardQuery request,
            CancellationToken cancellationToken)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            var monthBuckets = GetLastSixMonthBuckets(nowUtc);
            var monthLabels = monthBuckets.Select(x => x.Label).ToArray();

            var totalUsersTask = _searchApiProvider.GetTotalManagementUsersCountAsync(request.RequestInformation, cancellationToken);
            var usersTask = FetchAllManagementUsersAsync(request.RequestInformation, cancellationToken);

            await Task.WhenAll(totalUsersTask, usersTask);

            var totalUsersResult = await totalUsersTask;
            if (!totalUsersResult.IsSuccess)
            {
                return Result<bool>.Failure(totalUsersResult.ErrorCode, totalUsersResult.ErrorMessage);
            }

            var usersResult = await usersTask;
            if (!usersResult.IsSuccess)
            {
                return Result<bool>.Failure(usersResult.ErrorCode, usersResult.ErrorMessage);
            }

            var users = usersResult.Data;
            var totalUsers = totalUsersResult.Data;

            var createdAtUtc = users.Select(x => ToUtc(x.CreatedAt)).ToArray();

            var cumulativeByMonth = monthBuckets
                .Select(bucket => createdAtUtc.LongCount(d => d < bucket.EndExclusiveUtc))
                .ToArray();

            var monthlyRegistrations = monthBuckets
                .Select(bucket => createdAtUtc.LongCount(d => d >= bucket.StartUtc && d < bucket.EndExclusiveUtc))
                .ToArray();

            var activeUsers = users.LongCount(x => IsActiveStatus(x.Status));
            var inactiveUsers = users.Count - activeUsers;

            var thirtyDaysAgo = nowUtc.AddDays(-30);
            var newUsersLast30Days = createdAtUtc.LongCount(d => d >= thirtyDaysAgo);

            var statusLabels = new[] { "Active", "Inactive" };
            var statusCounts = new[] { activeUsers, inactiveUsers };

            var topDepartments = TakeTopCounts(
                users
                    .Select(u => string.IsNullOrWhiteSpace(u.Department) ? "Unspecified" : u.Department.Trim()),
                TopN);

            var roleCounts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in users)
            {
                var roles = u.Roles;
                if (roles is null || roles.Count == 0)
                {
                    continue;
                }

                foreach (var role in roles)
                {
                    if (string.IsNullOrWhiteSpace(role))
                    {
                        continue;
                    }

                    var key = role.Trim();
                    roleCounts.TryGetValue(key, out var n);
                    roleCounts[key] = n + 1;
                }
            }

            var topRoles = roleCounts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Take(TopN)
                .ToArray();

            analytics.ManagementUsers = new ManagementUserAnalyticsDto
            {
                AnalyticsKey = UserManagementDashboardAnalyticsKeys.ManagementUsers,
                Summary = new ManagementUserAnalyticsSummaryDto
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = inactiveUsers,
                    NewUsersLast30Days = newUsersLast30Days,
                },
                Charts = new ManagementUserAnalyticsChartsDto
                {
                    GrowthTrend = new ManagementUserGrowthTrendChartDto
                    {
                        Labels = monthLabels,
                        Datasets =
                        [
                            new ManagementUserDatasetDto
                            {
                                Label = "Total accounts (cumulative)",
                                Data = cumulativeByMonth,
                            },
                        ],
                    },
                    MonthlyRegistrations = new ManagementUserMonthlyCountsChartDto
                    {
                        Labels = monthLabels,
                        Counts = monthlyRegistrations,
                    },
                    StatusDistribution = new ManagementUserStatusDistributionChartDto
                    {
                        Labels = statusLabels,
                        Counts = statusCounts,
                    },
                    TopDepartments = new ManagementUserLabeledCountsChartDto
                    {
                        Labels = topDepartments.Labels,
                        Counts = topDepartments.Counts,
                    },
                    TopRoles = new ManagementUserLabeledCountsChartDto
                    {
                        Labels = topRoles.Select(x => x.Key).ToArray(),
                        Counts = topRoles.Select(x => x.Value).ToArray(),
                    },
                },
            };

            return Result<bool>.Success(true);
        }

        private async Task<Result<IReadOnlyList<ManagementUserSummary>>> FetchAllManagementUsersAsync(
            RequestInformation requestInformation,
            CancellationToken cancellationToken)
        {
            var all = new List<ManagementUserSummary>();
            var pageIndex = 1;
            while (true)
            {
                var searchRequest = new PaginatedSearchRequest
                {
                    SearchType = SearchType.All,
                    PageIndex = 1,
                    PageSize = SearchPageSize,
                    SortField = "createdAt",
                    SortOrder = "desc",
                    IsFilteredQuery = false,
                };

                var result = await _searchApiProvider.SearchManagementUsersAsync(searchRequest, requestInformation, cancellationToken);
                if (!result.IsSuccess)
                {
                    return Result<IReadOnlyList<ManagementUserSummary>>.Failure(result.ErrorCode, result.ErrorMessage);
                }

                var page = result.Data;
                var batch = page?.Data;
                if (batch is null || batch.Count == 0)
                {
                    break;
                }

                all.AddRange(batch);

                var total = page.Count;
                if (all.Count >= total || batch.Count < SearchPageSize)
                {
                    break;
                }

                pageIndex++;
            }

            return Result<IReadOnlyList<ManagementUserSummary>>.Success(all);
        }

        private static (string[] Labels, long[] Counts) TakeTopCounts(IEnumerable<string> values, int topN)
        {
            var map = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var v in values)
            {
                map.TryGetValue(v, out var n);
                map[v] = n + 1;
            }

            var top = map
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Take(topN)
                .ToArray();

            return (top.Select(x => x.Key).ToArray(), top.Select(x => x.Value).ToArray());
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
