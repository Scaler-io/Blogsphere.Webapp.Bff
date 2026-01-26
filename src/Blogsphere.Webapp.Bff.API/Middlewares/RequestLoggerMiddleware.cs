
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Context;
using Blogsphere.Webapp.Bff.Application.Extensions;

namespace Blogsphere.Webapp.Bff.API.Middlewares
{
    public class RequestLoggerMiddleware(ILogger logger) : IMiddleware
    {
        private readonly ILogger _logger = logger;

        private static readonly string[] HealthCheckPaths = ["/healthcheck", "/dashboard"];

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var isHealthCheckEndpoint = HealthCheckPaths.Any(path.Contains);

            Stopwatch stopWatch = new();
            stopWatch.Start();

            var headers = context.Request.Headers.ToDictionary(x => x.Key, x => x.Value);
            var url = context.Request.GetDisplayUrl();
            var verb = context.Request.Method;

            using (LogContext.PushProperty("Url", url))
            {
                LogContext.PushProperty("HttepMethod", verb);

                // Skip logging for health check endpoints
                if (!isHealthCheckEndpoint)
                {
                    _logger.Here().Information("Http request starting...");
                }

                await next(context);

                stopWatch.Stop();

                // Only log health check requests if they resulted in an error
                var isError = context.Response.StatusCode >= 400;

                if (!isHealthCheckEndpoint || isError)
                {
                    _logger.Here().Debug("Elapsed time {elapsedTime}", stopWatch.Elapsed);
                    _logger.Here().Information("Http request completed. Response code {@code}", context.Response.StatusCode);
                }
            }
        }
    }
}