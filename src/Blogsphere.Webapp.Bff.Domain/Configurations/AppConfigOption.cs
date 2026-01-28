namespace Blogsphere.Webapp.Bff.Domain.Configurations
{
    public class AppConfigOption
    {
        public const string OptionName = "AppConfigurations";
        public string ApplicationIdentifier { get; set; }
        public string ApplicationEnvironment { get; set; }
        public int CacheExpiration { get; set; }
    }
}