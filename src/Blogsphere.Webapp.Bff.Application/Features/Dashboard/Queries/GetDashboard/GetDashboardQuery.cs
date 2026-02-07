using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQuery(RequestInformation requestInformation) : IQuery<Result<DashboardDto>>
    {
        public RequestInformation RequestInformation { get; private set; } = requestInformation;
    }
}

