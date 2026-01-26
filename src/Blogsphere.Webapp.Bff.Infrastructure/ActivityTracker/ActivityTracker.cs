using System.Diagnostics;
using Blogsphere.Webapp.Bff.Application.Contracts.ActivityTracker;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Infrastructure.ActivityTracker
{
    public class ActivityTracker(ActivitySource activitySource) : ActivityTrackerBase, IActivityTracker
    {
        private readonly ActivitySource _activitySource = activitySource;
        protected override Activity StartActivity(string name, ActivityKind activityKind = ActivityKind.Internal, Action<Activity> configure = null)
        {
            var activity = _activitySource.StartActivity(name, activityKind);
            if (activity != null && configure != null)
            {
                configure(activity);
            }
            return activity;
        }

        public Activity TrackCommandActivity(string commandName, params (string key, object value)[] tags) => throw new NotImplementedException();
        public Activity TrackInMemoryActivity(string operationName, string cacheKey)
        {
            return StartActivity($"Inmemory caching {operationName}", ActivityKind.Client, activity =>
            {
                activity.SetTag(TrackerConstants.CacheType, nameof(CacheServiceType.InMemory));
                activity.SetTag(TrackerConstants.CacheOperation, operationName);
                activity.SetTag(TrackerConstants.CacheKey, cacheKey);
            });
        }
        public Activity TrackRedisActivity(string operationName, string cacheKey)
        {
            return StartActivity($"Redis caching {operationName}", ActivityKind.Client, activity =>
            {
                activity.SetTag(TrackerConstants.CacheType, nameof(CacheServiceType.Distributed));
                activity.SetTag(TrackerConstants.CacheOperation, operationName);
                activity.SetTag(TrackerConstants.CacheKey, cacheKey);
            });
        }
    }
}