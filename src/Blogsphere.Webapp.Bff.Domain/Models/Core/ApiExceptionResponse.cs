using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Domain.Models.Core;

public class ApiExceptionResponse(string errorMessage = "", string stackTrace = "") : ApiResponse(ErrorCode.InternalServerError, errorMessage)
{
    public string StackTrace { get; set; } = stackTrace;
}