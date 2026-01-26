using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.Swagger.Example.Common;

public class InternalServerErrorResponseExample : IExamplesProvider<ApiExceptionResponse>
{
    public ApiExceptionResponse GetExamples() => new(ErrorMessages.InternalServerError);
}
