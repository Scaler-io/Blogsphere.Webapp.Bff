using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Domain.Models.Core
{
    public class ApiValidationResponse : ApiResponse
    {
        public ApiValidationResponse(string errorMessage = "", string correlationId = null) : base(ErrorCode.BadRequest, errorMessage, correlationId)
        {
            ErrorMessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage : GetDefaultErrorMessage(Code);
        }
        public List<FieldLevelError> Errors { get; set; }
        protected override string GetDefaultErrorMessage(ErrorCode code)
        {
            return "Invalid data provided";
        }
    }
}