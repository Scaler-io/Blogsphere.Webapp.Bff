using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQuery(RequestInformation requestInformation, string scope) : IQuery<Result<DashboardResponseBase>>
    {
        public RequestInformation RequestInformation { get; private set; } = requestInformation;

        public string Scope { get; private set; } = scope;
    }
}
