namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard
{
    /// <summary>
    /// User-management dashboard. Add new analytics by extending <see cref="UserManagementDashboardAnalyticsDto"/>
    /// and registering a matching contributor in the application layer DI.
    /// </summary>
    public sealed class UserManagementDashboardDto : DashboardResponseBase
    {
        public override string Kind => DashboardScopes.UserManagement;

        /// <summary>
        /// Grouped analytics (management users, app users, etc.). Each section is populated independently.
        /// </summary>
        public UserManagementDashboardAnalyticsDto Analytics { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public int RefreshAfterSeconds { get; set; }
    }

    /// <summary>
    /// Container for all analytics domains on this dashboard. Add new properties here when new user domains
    /// need dedicated summaries and charts (e.g. <c>AppUsers</c>).
    /// </summary>
    public class UserManagementDashboardAnalyticsDto
    {
        public ManagementUserAnalyticsDto ManagementUsers { get; set; }

        // public AppUserAnalyticsDto AppUsers { get; set; }
    }

    public class ManagementUserAnalyticsDto
    {
        public string AnalyticsKey { get; set; } = UserManagementDashboardAnalyticsKeys.ManagementUsers;

        public ManagementUserAnalyticsSummaryDto Summary { get; set; }
        public ManagementUserAnalyticsChartsDto Charts { get; set; }
    }

    public class ManagementUserAnalyticsSummaryDto
    {
        public long TotalUsers { get; set; }
        public long ActiveUsers { get; set; }
        public long InactiveUsers { get; set; }
        public long NewUsersLast30Days { get; set; }
    }

    public class ManagementUserAnalyticsChartsDto
    {
        public ManagementUserGrowthTrendChartDto GrowthTrend { get; set; }
        public ManagementUserMonthlyCountsChartDto MonthlyRegistrations { get; set; }
        public ManagementUserStatusDistributionChartDto StatusDistribution { get; set; }
        public ManagementUserLabeledCountsChartDto TopDepartments { get; set; }
        public ManagementUserLabeledCountsChartDto TopRoles { get; set; }
    }

    public class ManagementUserGrowthTrendChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<ManagementUserDatasetDto> Datasets { get; set; }
    }

    public class ManagementUserDatasetDto
    {
        public string Label { get; set; }
        public IReadOnlyList<long> Data { get; set; }
    }

    public class ManagementUserMonthlyCountsChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<long> Counts { get; set; }
    }

    public class ManagementUserStatusDistributionChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<long> Counts { get; set; }
    }

    public class ManagementUserLabeledCountsChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<long> Counts { get; set; }
    }
}
