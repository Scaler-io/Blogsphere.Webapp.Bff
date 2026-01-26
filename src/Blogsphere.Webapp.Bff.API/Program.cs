using Blogsphere.Webapp.Bff.API.DI;
using Blogsphere.Webapp.Bff.Application.DI;
using Blogsphere.Webapp.Bff.API;
using Serilog;
using Blogsphere.Webapp.Bff.Swagger;
using Blogsphere.Webapp.Bff.Infrastructure.DI;

var builder = WebApplication.CreateBuilder(args);
var logger = Logging.GetLogger(builder.Configuration);
builder.Services.AddSingleton(logger);
builder.Host.UseSerilog(logger);

var apiName = SwaggerConfiguration.ExtractApiNameFromEnvironmentVariable();
var apiDescription = builder.Configuration["ApiDescription"];
var apiHost = builder.Configuration["ApiOriginHost"];
var swaggerConfiguration = new SwaggerConfiguration(apiName, apiDescription, apiHost, builder.Environment.IsDevelopment());

builder.Services
.AddConfigurationOptions(builder.Configuration)
.AddApplicationServices(builder.Configuration, swaggerConfiguration)
.AddBusinessLogicServices()
.AddInfrastructureServices(builder.Configuration)
.AddHttpClients(builder.Configuration);

var app = builder.Build();
app.AddApplicationPipeline();

#if DEBUG
builder.WebHost.UseUrls("http://localhost:5003");
#endif

try
{
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program for integration testing (WebApplicationFactory<Program>)
public partial class Program { }