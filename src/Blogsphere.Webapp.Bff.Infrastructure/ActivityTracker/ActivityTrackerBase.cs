using System.Diagnostics;

namespace Blogsphere.Webapp.Bff.Infrastructure.ActivityTracker
{
    public abstract class ActivityTrackerBase
    {
        protected abstract Activity StartActivity(string name, ActivityKind activityKind = ActivityKind.Internal, Action<Activity> configure = null);
    }
}