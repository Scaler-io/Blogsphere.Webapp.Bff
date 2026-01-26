using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using System.Threading;

namespace Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider
{
    public interface IApiGatewayProvider
    {
        Task<Result<ApiCluster>> GetApiCLusterDetailsByIdAsync(
            string id,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default);
    }
}
