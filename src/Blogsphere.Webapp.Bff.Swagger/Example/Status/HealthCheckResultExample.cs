using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.Swagger.Example.Status;

public class HealthCheckResultExample : IExamplesProvider<string>
{
    public string GetExamples() => "Healthy";
}
