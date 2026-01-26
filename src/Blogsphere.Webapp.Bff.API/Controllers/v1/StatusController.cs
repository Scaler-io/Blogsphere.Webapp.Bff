using Asp.Versioning;
using Blogsphere.Webapp.Bff.Application.Contracts.Security;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Swagger;
using Blogsphere.Webapp.Bff.Swagger.Example.Common;
using Blogsphere.Webapp.Bff.Swagger.Example.Status;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.API.Controllers.v1
{
    [ApiVersion("1")]
    public class StatusController(
        ILogger logger,
        IIdentityService identityService,
        HealthCheckService healthCheckService) : BaseApiController(logger, identityService)
    {
        private readonly HealthCheckService _healthCheckService = healthCheckService;

        [HttpGet]
        [SwaggerOperation(OperationId = "GetStatus", Description = "Get the health status of the application")]
        [SwaggerHeader("CorrelationId", Description = "Expected to be a valid and unique correlation id")]
        // 200
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(HealthCheckResultExample))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        // 500
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExample))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiExceptionResponse))]
        // 404
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(NotFoundResponseExample))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse))]
        public async Task<IActionResult> GetStatus()
        {
            var healthCheckResult = await _healthCheckService.CheckHealthAsync();
            return Ok(healthCheckResult.Status);
        }
    }
}