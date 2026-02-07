using Blogsphere.Webapp.Bff.Domain.Entities.Search;
using Blogsphere.Webapp.Bff.Domain.Models.Core;

namespace Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider
{
    public interface ISearchApiProvider
    {
        Task<Result<PaginatedResult<ApiClusterSummary>>> SearchClustersAsync(PaginatedSearchRequest request, RequestInformation requestInformation, CancellationToken cancellationToken = default);
        Task<Result<PaginatedResult<ApiRouteSummary>>> SearchRoutesAsync(PaginatedSearchRequest request, RequestInformation requestInformation, CancellationToken cancellationToken = default);

        Task<Result<long>> GetTotalClustersCountAsync(RequestInformation requestInformation, CancellationToken cancellationToken = default);
        Task<Result<long>> GetTotalRoutesCountAsync(RequestInformation requestInformation, CancellationToken cancellationToken = default);
    }
}
