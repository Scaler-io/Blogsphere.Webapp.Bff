using Blogsphere.Webapp.Bff.Domain.Entities;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Contracts.Enrichment
{
    public interface IMetaDataUserEnricher
    {
        Task<Result<MetaDataDto>> EnrichAsync(
            MetaData sourceMetaData,
            MetaDataDto targetMetaData,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default);
    }

}
