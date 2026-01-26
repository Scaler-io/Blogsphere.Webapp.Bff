using Serilog;
using Serilog.Events;

namespace Blogsphere.Webapp.Bff.API.Extensions
{
    public static class LoggerConfigurationExtensions
    {
        public static LoggerConfiguration OverrideLogLevels(this LoggerConfiguration loggerConfiguration) => loggerConfiguration
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.IdentityModel", LogEventLevel.Debug)
                .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.HealthChecks", LogEventLevel.Error)
                .MinimumLevel.Override("HealthChecks", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.Extensions.Diagnostics.HealthChecks", LogEventLevel.Error);
    }
}