using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Features.ApiRoute.Queries.GetApiRouteById
{
    public class GetApiRouteByIdQuery(string id, RequestInformation requestInformation) : IQuery<Result<ApiRouteDto>>
    {
        public string Id { get; private set; } = id;
        public RequestInformation RequestInformation { get; private set; } = requestInformation;
    }
}
