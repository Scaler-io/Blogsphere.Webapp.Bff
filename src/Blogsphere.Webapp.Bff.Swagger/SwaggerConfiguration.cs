using Asp.Versioning.ApiExplorer;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Blogsphere.Webapp.Bff.Swagger;

public sealed class SwaggerConfiguration
{
    private const string DefaultScheme = "http";
    private const string DefaultEnvironmentName = "Development";
    private readonly string _apiName;
    private readonly string _apiDescription;
    private static bool _isDevelopment;
    private static string _apiHost;

    public SwaggerConfiguration(string apiName, string apiDescription, string apiHost, bool isDevelopment)
    {
        _apiName = apiName;
        _apiDescription = apiDescription;
        _apiHost = apiHost;
        _isDevelopment = isDevelopment;
    }

    public static string ExtractApiNameFromEnvironmentVariable()
    {
        var environment = Environment.GetEnvironmentVariable(EnvironmentConstants.SwaggerEnvironmentName) ?? DefaultEnvironmentName;
        return $"Blogsphere.Webapp.Bff.{environment}".Trim();
    }

    public static void SetupSwaggerOptions(SwaggerOptions options)
    {
        var scheme = Environment.GetEnvironmentVariable(EnvironmentConstants.SwaggerScheme) ?? DefaultScheme;
        options.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            swagger.Servers.Clear();
            swagger.Servers.Add(new OpenApiServer { Url = $"{scheme}://{_apiHost ?? "localhost"}" });
            swagger.Servers.Add(new OpenApiServer { Url = $"https://{_apiHost ?? "localhost"}" });
        });
        options.SerializeAsV2 = true;
    }

    public void SetupSwaggerGenOptions(SwaggerGenOptions options, IApiVersionDescriptionProvider provider)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = _apiName,
                Version = description.GroupName,
                Description = _apiDescription,
            });
        }

        // add filters for the swagger docs
        options.DocumentFilter<SwaggerBasePath>();
        // options.DocumentFilter<SwaggerRemoveVersionFromRoute>();
        options.UseInlineDefinitionsForEnums();
        options.ExampleFilters();
        options.OperationFilter<SwaggerHeaderFilter>();
        options.SchemaFilter<EnumSchemaFilter>();
        options.CustomSchemaIds(x => x.FullName);
        options.EnableAnnotations();
        if (_isDevelopment)
        {
            options.OperationFilter<SwaggerApiVersionFilter>();
        }
    }

    public static void SetupSwaggerUiOptions(SwaggerUIOptions options, IApiVersionDescriptionProvider provider)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            $"Blogsphere.Webapp.Bff {description.GroupName.ToUpperInvariant()}"
            );
        }
    }

    public static void SetupScalarOptions(ScalarOptions options, ApiVersionDescription description)
    {
        options.WithOpenApiRoutePattern($"swagger/{description.GroupName}/swagger.json")
        .WithTitle($"Blogsphere.Webapp.Bff {description.GroupName.ToUpperInvariant()}")
        .WithDarkModeToggle()
        .WithTheme(ScalarTheme.Default)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Http)
        .WithDefaultFonts()
        .WithLayout(ScalarLayout.Modern);
    }
}