using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Domain.Models.Core
{
    public class ApiExceptionResponse(string errorMessage = "", string stackTrace = "", string correlationId = null) : ApiResponse(ErrorCode.InternalServerError, errorMessage, correlationId)
    {
        public string StackTrace { get; set; } = stackTrace;
    }
}