using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard
{
    public interface IDashboardScopeHandler
    {
        string Scope { get; }
        Task<Result<DashboardResponseBase>> HandleAsync(GetDashboardQuery request, CancellationToken cancellationToken);
    }
}
