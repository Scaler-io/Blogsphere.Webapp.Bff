using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Blogsphere.Webapp.Bff.API.Middlewares;
using Blogsphere.Webapp.Bff.Domain.Configurations;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Swagger;
using Blogsphere.Webapp.Bff.Swagger.Example.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Filters;

namespace Blogsphere.Webapp.Bff.API.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, SwaggerConfiguration swaggerConfiguration)
        {
            services.AddControllers()
                .AddNewtonsoftJson(config =>
                {
                    config.SerializerSettings.ContractResolver = new DefaultContractResolver()
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                    config.SerializerSettings.NullValueHandling = NullValueHandling.Include;
                    config.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            services.AddSingleton(new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore,
                Converters = [new StringEnumConverter()]
            });

            services.AddEndpointsApiExplorer();
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = ApiVersion.Default;
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddSwaggerExamplesFromAssemblies(typeof(ValidationResponseExample).Assembly);
            services.AddSwaggerExamples();
            services.AddSwaggerGen(options =>
            {
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                swaggerConfiguration.SetupSwaggerGenOptions(options, provider);
            });

            services.AddHealthChecks();
            services.AddHealthChecksUI(options =>
            {
                options.AddHealthCheckEndpoint("Blogsphere.Webapp.Bff.Api", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker" ? "http://host.docker.internal:8003/healthcheck" : "http://localhost:5003/healthcheck");
            })
            .AddInMemoryStorage();

            services.AddHttpContextAccessor();

            var identityGroupAccess = configuration.GetSection(IdentityGroupAccessOption.OptionName)
            .Get<IdentityGroupAccessOption>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = identityGroupAccess.Audience;
                    options.Authority = identityGroupAccess.Authority;
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = identityGroupAccess.Authority,
                        ValidAudience = identityGroupAccess.Audience
                    };
                });

            services.AddAuthorizationBuilder()
                .AddDefaultPolicy("BffApiPolicy", policy =>
                {
                    policy.RequireClaim("scope", "bffapi:manage");
                });

            services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                var serviceName = configuration["AppConfigurations:ApplicationIdentifier"] ?? "Blogsphere.Webapp.Bff.Api";
                resource.AddService(serviceName);
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource("Blogsphere.Webapp.Bff.Api")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRedisInstrumentation()
                .AddJaegerExporter(cfg =>
                {
                    cfg.AgentHost = configuration["Jaeger:Host"];
                    cfg.AgentPort = int.Parse(configuration["Jaeger:Port"]);
                });
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = HandleFrameworkValidationFailure();
            });

            services.AddTransient<CorrelationHeaderEnricher>();
            services.AddTransient<RequestLoggerMiddleware>();
            services.AddTransient<GlobalExceptionMiddleware>();

            return services;
        }

        private static Func<ActionContext, IActionResult> HandleFrameworkValidationFailure()
        {
            return context =>
            {
                var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .ToList();

                var validationError = new ApiValidationResponse
                {
                    Errors = []
                };

                foreach (var error in errors)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        validationError.Errors.Add(new FieldLevelError
                        {
                            Field = error.Key,
                            Message = subError.ErrorMessage
                        });
                    }
                }
                return new BadRequestObjectResult(validationError);
            };
        }
    }
}