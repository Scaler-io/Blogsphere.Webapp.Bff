using Newtonsoft.Json;

namespace Blogsphere.Webapp.Bff.Infrastructure.Security
{
    public class IdentityBase
    {
        protected static IReadOnlyList<string> ParseClaimValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return [];
            }

            if (value == "*")
            {
                return ["*"];
            }

            if (value.Trim().StartsWith("["))
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(value) ?? [];
                }
                catch (JsonException)
                {
                    return [value];
                }
            }
            return [value];
        }
    }
}