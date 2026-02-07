using Asp.Versioning;
using Blogsphere.Webapp.Bff.Application.Contracts.Security;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Blogsphere.Webapp.Bff.Swagger;

namespace Blogsphere.Webapp.Bff.API.Controllers.v2.Dashboard
{
    [ApiVersion("2")]
    [Authorize]
    public class DashboardController(
        ILogger logger,
        IIdentityService identityService,
        IMediator mediator) : BaseApiController(logger, identityService)
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        [SwaggerOperation(OperationId = "GetDashboard", Description = "Get dashboard summary and charts")]
        [SwaggerHeader("CorrelationId", Description = "Expected to be a valid and unique correlation id")]
        // 200
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardDto))]
        // 400
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiValidationResponse))]
        // 401
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse))]
        // 404
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        // 500
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiExceptionResponse))]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            Logger.Here().MethodEntered();
            var query = new GetDashboardQuery(RequestInformation);
            var result = await _mediator.Send(query, cancellationToken);
            Logger.Here().MethodExited();
            return OkOrFailure(result);
        }
    }
}

