using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById
{
    public class GetApiClusterByIdQuery(string id, RequestInformation requestInformation) : IQuery<Result<ApiClusterDto>>
    {
        public string Id { get; set; } = id;
        public RequestInformation RequestInformation { get; set; } = requestInformation;
    }
}
