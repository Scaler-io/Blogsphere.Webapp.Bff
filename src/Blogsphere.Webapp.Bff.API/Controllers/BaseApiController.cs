using Blogsphere.Webapp.Bff.API.Extensions;
using Blogsphere.Webapp.Bff.Application.Contracts.Security;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Blogsphere.Webapp.Bff.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BaseApiController(ILogger logger, IIdentityService identityService) : ControllerBase
    {
        protected ILogger Logger { get; private set; } = logger;
        protected IIdentityService IdentityService { get; private set; } = identityService;

        protected RequestInformation RequestInformation => new()
        {
            CorreationId = GetOrGenerateCorrelationId(),
            CurrentUser = IdentityService.PrepareUser()
        };

        private string GetOrGenerateCorrelationId() => Request?.GetRequestHeaderOrDefault("CorrelationId", $"GEN-{Guid.NewGuid()}");

        protected IActionResult OkOrFailure<T>(Result<T> result)
        {
            if (result == null)
            {
                return NotFound(new ApiResponse(ErrorCode.NotFound, correlationId: RequestInformation.CorreationId));
            }

            if (result.IsSuccess && result.Data == null)
            {
                return NotFound(new ApiResponse(ErrorCode.NotFound, correlationId: RequestInformation.CorreationId));
            }

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(result.Data);
            }

            return result.ErrorCode switch
            {
                ErrorCode.BadRequest => BadRequest(new ApiValidationResponse(result.ErrorMessage, correlationId: RequestInformation.CorreationId)),
                ErrorCode.InternalServerError => InternalServerError(new ApiExceptionResponse(result.ErrorMessage, correlationId: RequestInformation.CorreationId)),
                ErrorCode.NotFound => NotFound(new ApiResponse(ErrorCode.NotFound, result.ErrorMessage, correlationId: RequestInformation.CorreationId)),
                ErrorCode.Unauthorized => Unauthorized(new ApiResponse(ErrorCode.Unauthorized, result.ErrorMessage, correlationId: RequestInformation.CorreationId)),
                ErrorCode.OperationFailed => BadRequest(new ApiResponse(ErrorCode.OperationFailed, result.ErrorMessage, correlationId: RequestInformation.CorreationId)),
                ErrorCode.NotAllowed => BadRequest(new ApiResponse(ErrorCode.NotAllowed, result.ErrorMessage, correlationId: RequestInformation.CorreationId)),
                _ => BadRequest(new ApiResponse(ErrorCode.BadRequest, result.ErrorMessage, correlationId: RequestInformation.CorreationId))
            };

        }
        private static ObjectResult InternalServerError(ApiResponse response)
        {
            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ContentTypes =
                [
                    "application/json"
                ]
            };
        }
    }
}