namespace Blogsphere.Webapp.Bff.Domain.Models.Dtos
{
    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; }
        public DashboardChartsDto Charts { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public int RefreshAfterSeconds { get; set; }
    }

    public class DashboardSummaryDto
    {
        public long Clusters { get; set; }
        public long Routes { get; set; }
        public long Products { get; set; }
    }

    public class DashboardChartsDto
    {
        public DashboardGrowthTrendChartDto GrowthTrend { get; set; }
        public DashboardStatusDistributionChartDto ClusterStatusDistribution { get; set; }
        public DashboardMonthlyCountsChartDto MonthlyRouteActivity { get; set; }
    }

    public class DashboardGrowthTrendChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<DashboardDatasetDto> Datasets { get; set; }
    }

    public class DashboardDatasetDto
    {
        public string Label { get; set; }
        public IReadOnlyList<long> Data { get; set; }
    }

    public class DashboardStatusDistributionChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<long> Counts { get; set; }
    }

    public class DashboardMonthlyCountsChartDto
    {
        public IReadOnlyList<string> Labels { get; set; }
        public IReadOnlyList<long> Counts { get; set; }
    }
}

