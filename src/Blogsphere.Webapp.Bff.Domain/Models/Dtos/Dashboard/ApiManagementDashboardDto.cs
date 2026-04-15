namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard
{
    public sealed class ApiManagementDashboardDto : DashboardResponseBase
    {
        public override string Kind => DashboardScopes.ApiManagement;

        public ApiManagementDashboardSummaryDto Summary { get; set; }
        public ApiManagementDashboardChartsDto Charts { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int RefreshAfterSeconds { get; set; }
    }

    public class ApiManagementDashboardSummaryDto
    {
        public long Clusters { get; set; }
        public long Routes { get; set; }
        public long Products { get; set; }
    }

    public class ApiManagementDashboardChartsDto
    {
        public ApiManagementGrowthTrendChartDto GrowthTrend { get; set; }
        public ApiManagementStatusDistributionChartDto ClusterStatusDistribution { get; set; }
        public ApiManagementMonthlyCountsChartDto MonthlyRouteActivity { get; set; }
    }

    public class ApiManagementGrowthTrendChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<ApiManagementDatasetDto> Datasets { get; set; }
    }

    public class ApiManagementDatasetDto
    {
        public string Label { get; set; }
        public IReadOnlyList<long> Data { get; set; }
    }

    public class ApiManagementStatusDistributionChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<long> Counts { get; set; }
    }

    public class ApiManagementMonthlyCountsChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<long> Counts { get; set; }
    }
}
