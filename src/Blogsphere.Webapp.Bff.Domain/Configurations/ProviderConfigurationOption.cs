namespace Blogsphere.Webapp.Bff.Domain.Configurations;

public class ProviderConfigurationOption
{
    public const string OptionName = "ProviderSettings";
    public UserApiSettings UserApiSettings { get; set; }
    public ApiGatewaySettings ApiGatewaySettings { get; set; }
    public BffApiSettings BffApiSettings { get; set; }
}

public class ApiSettings
{
    public string BaseUrl { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string SubscriptionKey { get; set; }
}

public class UserApiSettings : ApiSettings {}

public class ApiGatewaySettings : ApiSettings {}

public class BffApiSettings : ApiSettings {}