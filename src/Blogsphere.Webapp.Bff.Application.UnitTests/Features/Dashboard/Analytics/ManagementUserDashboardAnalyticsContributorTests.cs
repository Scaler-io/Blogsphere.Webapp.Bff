using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Analytics;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Entities.Search;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using FluentAssertions;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Features.Dashboard.Analytics;

public class ManagementUserDashboardAnalyticsContributorTests
{
    [Fact]
    public async Task ContributeAsync_propagates_failure_when_total_count_fails()
    {
        var searchApi = new Mock<ISearchApiProvider>();
        searchApi
            .Setup(s => s.GetTotalManagementUsersCountAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Failure(ErrorCode.OperationFailed, "count failed"));
        var emptyPage = new PaginatedResult<ManagementUserSummary>(1, 500, 0, []);
        searchApi
            .Setup(s => s.SearchManagementUsersAsync(It.IsAny<PaginatedSearchRequest>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<ManagementUserSummary>>.Success(emptyPage));

        var contributor = new ManagementUserDashboardAnalyticsContributor(searchApi.Object);
        var analytics = new UserManagementDashboardAnalyticsDto();
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), "user-management");

        var result = await contributor.ContributeAsync(analytics, request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.OperationFailed);
        result.ErrorMessage.Should().Be("count failed");
    }

    [Fact]
    public async Task ContributeAsync_propagates_failure_when_search_users_fails()
    {
        var searchApi = new Mock<ISearchApiProvider>();
        searchApi
            .Setup(s => s.GetTotalManagementUsersCountAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Success(0));
        searchApi
            .Setup(s => s.SearchManagementUsersAsync(It.IsAny<PaginatedSearchRequest>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<ManagementUserSummary>>.Failure(ErrorCode.OperationFailed, "search failed"));

        var contributor = new ManagementUserDashboardAnalyticsContributor(searchApi.Object);
        var analytics = new UserManagementDashboardAnalyticsDto();
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), "user-management");

        var result = await contributor.ContributeAsync(analytics, request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.OperationFailed);
        result.ErrorMessage.Should().Be("search failed");
    }

    [Fact]
    public async Task ContributeAsync_populates_management_users_analytics_from_search_results()
    {
        var fixedUtc = new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);
        var user = new ManagementUserSummary
        {
            Id = "m1",
            Status = "Active",
            Department = "Engineering",
            Roles = ["Admin"],
            CreatedAt = fixedUtc,
            UpdatedAt = fixedUtc,
        };
        var page = new PaginatedResult<ManagementUserSummary>(1, 500, 1, [user]);

        var searchApi = new Mock<ISearchApiProvider>();
        searchApi
            .Setup(s => s.GetTotalManagementUsersCountAsync(It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long>.Success(1));
        searchApi
            .Setup(s => s.SearchManagementUsersAsync(It.IsAny<PaginatedSearchRequest>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginatedResult<ManagementUserSummary>>.Success(page));

        var contributor = new ManagementUserDashboardAnalyticsContributor(searchApi.Object);
        var analytics = new UserManagementDashboardAnalyticsDto();
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), "user-management");

        var result = await contributor.ContributeAsync(analytics, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        analytics.ManagementUsers.Should().NotBeNull();
        analytics.ManagementUsers!.Summary.TotalUsers.Should().Be(1);
        analytics.ManagementUsers.Summary.ActiveUsers.Should().Be(1);
        analytics.ManagementUsers.Summary.InactiveUsers.Should().Be(0);
        analytics.ManagementUsers.Charts.GrowthTrend.Labels.Should().HaveCount(6);
        analytics.ManagementUsers.Charts.StatusDistribution.Labels.Should().Equal("Active", "Inactive");
        analytics.ManagementUsers.Charts.TopDepartments.Labels.Should().Contain("Engineering");
        analytics.ManagementUsers.Charts.TopRoles.Labels.Should().Contain("Admin");
    }
}
