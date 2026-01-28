using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.Swagger.Example.Common
{
    public class UnAuthorizedResponseExample : IExamplesProvider<ApiResponse>
    {
        public ApiResponse GetExamples() => new(ErrorCode.Unauthorized, ErrorMessages.Unauthorized, correlationId: $"GEN-{Guid.NewGuid()}");
    }
}
