using AutoMapper;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Application.Contracts.Enrichment;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Features.ApiRoute.Queries.GetApiRouteById
{
    public class GetApiRouteByIdQueryHandler(
        ILogger logger,
        IApiGatewayProvider apiGatewayProvider,
        IMetaDataUserEnricher metaDataUserEnricher,
        IMapper mapper) : IQueryHandler<GetApiRouteByIdQuery, Result<ApiRouteDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IApiGatewayProvider _apiGatewayProvider = apiGatewayProvider;
        private readonly IMetaDataUserEnricher _metaDataUserEnricher = metaDataUserEnricher;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<ApiRouteDto>> Handle(GetApiRouteByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.Here().MethodEntered();
            _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Information("Getting api route by id {Id}", request.Id);

            var apiRoute = await _apiGatewayProvider.GetApiRouteByIdAsync(request.Id, request.RequestInformation, cancellationToken);
            if (!apiRoute.IsSuccess)
            {
                _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Error("Failed to get api route by id {Id}", request.Id);
                return Result<ApiRouteDto>.Failure(apiRoute.ErrorCode, apiRoute.ErrorMessage);
            }

            var apiRouteDto = _mapper.Map<ApiRouteDto>(apiRoute.Data);
            apiRouteDto.Metadata ??= new MetaDataDto();

            var enrichMetaResult = await _metaDataUserEnricher.EnrichAsync(apiRoute.Data.Metadata, apiRouteDto.Metadata, request.RequestInformation, cancellationToken);

            if (!enrichMetaResult.IsSuccess)
            {
                return Result<ApiRouteDto>.Failure(enrichMetaResult.ErrorCode, enrichMetaResult.ErrorMessage);
            }

            apiRouteDto.Metadata = enrichMetaResult.Data;
            _logger.Here().WithCorrelationId(request.RequestInformation.CorreationId).Information("Api route {Id} found", request.Id);
            _logger.Here().MethodExited(apiRouteDto);

            return Result<ApiRouteDto>.Success(apiRouteDto);
        }
    }
}
