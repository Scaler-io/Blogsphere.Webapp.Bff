using System.Diagnostics;

namespace Blogsphere.Webapp.Bff.Application.Contracts.ActivityTracker
{
    public interface IActivityTracker
    {
        Activity TrackRedisActivity(string operationName, string cacheKey);
        Activity TrackInMemoryActivity(string operationName, string cacheKey);
        Activity TrackCommandActivity(string commandName, params (string key, object value)[] tags);
    }
}