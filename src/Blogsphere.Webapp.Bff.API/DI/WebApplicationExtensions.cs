using Asp.Versioning.ApiExplorer;
using Blogsphere.Webapp.Bff.Swagger;
using HealthChecks.UI.Client;
using Scalar.AspNetCore;
using Blogsphere.Webapp.Bff.API.Middlewares;

namespace Blogsphere.Webapp.Bff.API.DI
{
    public static class WebApplicationExtensions
    {
        public static WebApplication AddApplicationPipeline(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            app.UseSwagger(SwaggerConfiguration.SetupSwaggerOptions);
            app.UseSwaggerUI(options =>
            {
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                SwaggerConfiguration.SetupSwaggerUiOptions(options, provider);

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    app.MapScalarApiReference($"scalar/{description.GroupName}", options =>
                    {
                        SwaggerConfiguration.SetupScalarOptions(options, description);
                    });
                }
            });

            app.UseMiddleware<CorrelationHeaderEnricher>()
            .UseMiddleware<RequestLoggerMiddleware>()
            .UseMiddleware<GlobalExceptionMiddleware>();

            app.MapHealthChecks("healthcheck", new()
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            app.MapHealthChecksUI(options => options.UIPath = "/dashboard");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}