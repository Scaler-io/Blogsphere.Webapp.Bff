using Asp.Versioning;
using Blogsphere.Webapp.Bff.Application.Contracts.Security;
using Microsoft.AspNetCore.Mvc;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using Blogsphere.Webapp.Bff.Swagger;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Swashbuckle.AspNetCore.Filters;
using Blogsphere.Webapp.Bff.Swagger.Example.Common;
using Microsoft.AspNetCore.Authorization;
using Blogsphere.Webapp.Bff.Application.Features.ApiRoute.Queries.GetApiRouteById;

namespace Blogsphere.Webapp.Bff.API.Controllers.v2.ApiGateway
{
    [ApiVersion("2")]
    [Authorize]

    public class ApiGatewayController(
        ILogger logger,
        IIdentityService identityService,
        IMediator mediator) : BaseApiController(logger, identityService)
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("cluster/{id}")]
        [SwaggerOperation(OperationId = "GetApiClusterById", Description = "Get an api cluster by id")]
        [SwaggerHeader("CorrelationId", Description = "Expected to be a valid and unique correlation id")]
        // 200
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiClusterDto))]
        // 400
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ValidationResponseExample))]
        // 401  
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnAuthorizedResponseExample))]
        // 404
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        // 500
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiExceptionResponse))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
        public async Task<IActionResult> GetApiClusterById([FromRoute] string id)
        {
            Logger.Here().MethodEntered();
            var query = new GetApiClusterByIdQuery(id, RequestInformation);
            var result = await _mediator.Send(query);
            Logger.Here().MethodExited();
            return OkOrFailure(result);
        }

        [HttpGet("route/{id}")]
        [SwaggerOperation(OperationId = "GetApiRouteById", Description = "Get an api route by id")]
        [SwaggerHeader("CorrelationId", Description = "Expected to be a valid and unique correlation id")]
        // 200
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiRouteDto))]
        // 400
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationResponse))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(ValidationResponseExample))]
        // 401  
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(UnAuthorizedResponseExample))]
        // 404
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        // 500
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiExceptionResponse))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
        public async Task<IActionResult> GetApiRouteById([FromRoute] string id)
        {
            Logger.Here().MethodEntered();
            var query = new GetApiRouteByIdQuery(id, RequestInformation);
            var result = await _mediator.Send(query);
            Logger.Here().MethodExited();
            return OkOrFailure(result);
        }
    }
}
