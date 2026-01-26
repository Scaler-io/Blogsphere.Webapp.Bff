using Blogsphere.Webapp.Bff.Domain.Configurations;

namespace Blogsphere.Webapp.Bff.API.DI
{
    public static class ServiceCollectionConfigurationExtensions
    {
        public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<AppConfigOption>().Bind(configuration.GetSection(AppConfigOption.OptionName));
            services.AddOptions<LoggingOption>().Bind(configuration.GetSection(LoggingOption.OptionName));
            services.AddOptions<ElasticSearchOption>().Bind(configuration.GetSection(ElasticSearchOption.OptionName));
            services.AddOptions<IdentityGroupAccessOption>().Bind(configuration.GetSection(IdentityGroupAccessOption.OptionName));
            services.AddOptions<ProviderConfigurationOption>().Bind(configuration.GetSection(ProviderConfigurationOption.OptionName));
            return services;
        }
    }
}