using Blogsphere.Webapp.Bff.API.Extensions;
using Blogsphere.Webapp.Bff.Domain.Configurations;
using Destructurama;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Blogsphere.Webapp.Bff.API
{
    public static class Logging
    {
        public static ILogger GetLogger(IConfiguration configuration)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var loggingOptions = configuration.GetSection(LoggingOption.OptionName).Get<LoggingOption>();
            var appConfigOptions = configuration.GetSection(AppConfigOption.OptionName).Get<AppConfigOption>();
            var elasticSearchOptions = configuration.GetSection(ElasticSearchOption.OptionName).Get<ElasticSearchOption>();

            var logIndexPattern = $"Blogsphere.Webapp.Bff.Api-{environment?.ToLower().Replace(".", "-")}";

            Enum.TryParse(loggingOptions.Console.LogLevel, false, out LogEventLevel minimumConsoleLogLevel);
            Enum.TryParse(loggingOptions.Elastic.LogLevel, false, out LogEventLevel minimumElasticLogLevel);

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .OverrideLogLevels()
                .Enrich.FromLogContext()
                .Enrich.WithProperty(nameof(Environment.MachineName), Environment.MachineName)
                .Enrich.WithProperty(nameof(appConfigOptions.ApplicationIdentifier), appConfigOptions.ApplicationIdentifier)
                .Enrich.WithProperty(nameof(appConfigOptions.ApplicationEnvironment), appConfigOptions.ApplicationEnvironment);

            if (loggingOptions.Console.Enabled)
            {
                loggerConfiguration.WriteTo.Console(
                    restrictedToMinimumLevel: minimumConsoleLogLevel,
                    outputTemplate: loggingOptions.LogOutputTemplate,
                    theme: AnsiConsoleTheme.Literate
                );
            }

            if (loggingOptions.Elastic.Enabled)
            {
                loggerConfiguration.WriteTo.Elasticsearch(
                    nodeUris: elasticSearchOptions.Uri,
                    indexFormat: logIndexPattern,
                    restrictedToMinimumLevel: minimumElasticLogLevel
                );
            }

            return loggerConfiguration
                .Destructure
                .UsingAttributes()
                .CreateLogger();
        }

    }
}