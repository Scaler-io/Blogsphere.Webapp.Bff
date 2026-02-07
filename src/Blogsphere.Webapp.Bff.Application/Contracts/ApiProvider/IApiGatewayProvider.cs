using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider
{
    public interface IApiGatewayProvider
    {
        Task<Result<ApiCluster>> GetApiCLusterDetailsByIdAsync(
            string id,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default);

        Task<Result<ApiRoute>> GetApiRouteByIdAsync(
            string id,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default);

        Task<Result<ApiProductListDto>> GetApiProductsAsync(
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default);
    }
}
