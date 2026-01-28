using AutoMapper;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Application.Contracts.Enrichment;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById
{
    public class GetApiClusterByIdQueryHandler(
        ILogger logger,
        IApiGatewayProvider apiGatewayProvider,
        IMetaDataUserEnricher metaDataUserEnricher,
        IMapper mapper) : IQueryHandler<GetApiClusterByIdQuery, Result<ApiClusterDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IApiGatewayProvider _apiGatewayProvider = apiGatewayProvider;
        private readonly IMetaDataUserEnricher _metaDataUserEnricher = metaDataUserEnricher;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<ApiClusterDto>> Handle(GetApiClusterByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.Here().MethodEntered();
            _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Information("Getting api cluster by id: {Id}", request.Id);

            var apiCluster = await _apiGatewayProvider.GetApiCLusterDetailsByIdAsync(request.Id, request.RequestInformation, cancellationToken);

            if (!apiCluster.IsSuccess)
            {
                _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Error("Failed to get api cluster by id: {Id}", request.Id);
                return Result<ApiClusterDto>.Failure(apiCluster.ErrorCode, apiCluster.ErrorMessage);
            }

            var apiClusterDto = _mapper.Map<ApiClusterDto>(apiCluster.Data);
            apiClusterDto.MetaData ??= new MetaDataDto();

            var enrichMetaResult = await _metaDataUserEnricher.EnrichAsync(
                apiCluster.Data.MetaData,
                apiClusterDto.MetaData,
                request.RequestInformation,
                cancellationToken);

            if (!enrichMetaResult.IsSuccess)
            {
                return Result<ApiClusterDto>.Failure(enrichMetaResult.ErrorCode, enrichMetaResult.ErrorMessage);
            }

            apiClusterDto.MetaData = enrichMetaResult.Data;

            _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Information("Api cluster {Id} found", request.Id);
            _logger.Here().MethodExited();
            return Result<ApiClusterDto>.Success(apiClusterDto);
        }
    }
}
