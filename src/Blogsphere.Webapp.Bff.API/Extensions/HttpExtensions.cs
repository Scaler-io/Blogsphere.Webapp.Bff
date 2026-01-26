namespace Blogsphere.Webapp.Bff.API.Extensions
{
    public static class HttpExtensions
    {
        public static string GetRequestHeaderOrDefault(this HttpRequest request, string key, string defaultValue = "")
        {
            var header = request?.Headers?.FirstOrDefault(h => h.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)).Value.FirstOrDefault();
            return header ?? defaultValue;
        }
    }
}