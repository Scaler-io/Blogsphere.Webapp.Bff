using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Domain.Models.Core
{
    public class ApiResponse
    {
        public ErrorCode Code { get; set; }
        public string ErrorMessage { get; set; }
        public string CorrelationId { get; set; }

        public ApiResponse(ErrorCode code, string errorMessage = null, string correlationId = null)
        {
            Code = code;
            ErrorMessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : GetDefaultErrorMessage(code);
            CorrelationId = correlationId;
        }

        protected virtual string GetDefaultErrorMessage(ErrorCode code)
        {
            return code switch
            {
                ErrorCode.BadRequest => ErrorMessages.BadRequest,
                ErrorCode.NotFound => ErrorMessages.NotFound,
                ErrorCode.Unauthorized => ErrorMessages.Unauthorized,
                ErrorCode.OperationFailed => ErrorMessages.Operationfailed,
                ErrorCode.InternalServerError => ErrorMessages.InternalServerError,
                ErrorCode.NotAllowed => ErrorMessages.NotAllowed,
                _ => string.Empty,
            };
        }
    }
}