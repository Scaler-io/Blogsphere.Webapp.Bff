using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.Swagger.Example.Common
{
    public class ValidationResponseExample : IExamplesProvider<ApiValidationResponse>
    {
        public ApiValidationResponse GetExamples() => new("Invalid data provided", correlationId: $"GEN-{Guid.NewGuid()}")
        {
            Errors = [
                new FieldLevelError
                {
                    Code = "Invalid",
                    Message = "The field Name is required",
                    Field = "Name"
                },
                new FieldLevelError
                {
                    Code = "Invalid",
                    Message = "The field Email is required",
                    Field = "Email"
                }
            ]
        };
    }
}
