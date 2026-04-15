using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard.Analytics
{
    public interface IUserManagementDashboardAnalyticsContributor
    {
        Task<Result<bool>> ContributeAsync(
            UserManagementDashboardAnalyticsDto analytics,
            GetDashboardQuery request,
            CancellationToken cancellationToken);
    }
}
