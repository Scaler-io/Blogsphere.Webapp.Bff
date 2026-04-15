using Blogsphere.Webapp.Bff.Application.Contracts.Caching;
using Blogsphere.Webapp.Bff.Application.Contracts.Factory;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Analytics;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard.Scopes
{
    public class UserManagementDashboardScopeHandler(
        ILogger logger,
        IEnumerable<IUserManagementDashboardAnalyticsContributor> analyticsContributors,
        ICacheServiceFactory cacheServiceFactory) : IDashboardScopeHandler
    {
        private readonly ILogger _logger = logger;
        private readonly IUserManagementDashboardAnalyticsContributor[] _analyticsContributors = [.. analyticsContributors];
        private readonly ICacheService _cacheService = cacheServiceFactory.GetCacheService(CacheServiceType.Distributed);

        public string Scope => DashboardScopes.UserManagement;

        public async Task<Result<DashboardResponseBase>> HandleAsync(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            _logger.Here().MethodEntered();

            var cacheKey = $"dashboard:{DashboardScopes.UserManagement}";
            var cached = await _cacheService.GetAsync<UserManagementDashboardDto>(cacheKey, cancellationToken);
            if (cached != null)
            {
                return Result<DashboardResponseBase>.Success(cached);
            }

            var nowUtc = DateTimeOffset.UtcNow;
            var analytics = new UserManagementDashboardAnalyticsDto();

            var outcomeTasks = _analyticsContributors
                .Select(c => c.ContributeAsync(analytics, request, cancellationToken));
            var outcomes = await Task.WhenAll(outcomeTasks);

            foreach (var outcome in outcomes)
            {
                if (!outcome.IsSuccess)
                {
                    return Result<DashboardResponseBase>.Failure(outcome.ErrorCode, outcome.ErrorMessage);
                }
            }

            var dashboard = new UserManagementDashboardDto
            {
                Analytics = analytics,
                Timestamp = nowUtc,
                RefreshAfterSeconds = 120,
            };

            await _cacheService.SetAsync(cacheKey, dashboard, 120, cancellationToken);

            _logger.Here().MethodExited();
            return Result<DashboardResponseBase>.Success(dashboard);
        }
    }
}
