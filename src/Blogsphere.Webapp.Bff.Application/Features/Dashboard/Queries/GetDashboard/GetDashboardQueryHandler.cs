using Blogsphere.Webapp.Bff.Application.Contracts.CQRS;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;

namespace Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQueryHandler(
        ILogger logger,
        IEnumerable<IDashboardScopeHandler> scopeHandlers) : IQueryHandler<GetDashboardQuery, Result<DashboardResponseBase>>
    {
        private readonly ILogger _logger = logger;
        private readonly IReadOnlyDictionary<string, IDashboardScopeHandler> _handlersByScope =
            scopeHandlers.ToDictionary(h => h.Scope, StringComparer.OrdinalIgnoreCase);

        public async Task<Result<DashboardResponseBase>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            _logger.Here().MethodEntered();

            if (string.IsNullOrWhiteSpace(request.Scope))
            {
                return Result<DashboardResponseBase>.Failure(ErrorCode.BadRequest, "The scope query parameter is required.");
            }

            var normalizedScope = request.Scope.Trim();
            if (!_handlersByScope.TryGetValue(normalizedScope, out var handler))
            {
                return Result<DashboardResponseBase>.Failure(ErrorCode.BadRequest, "Unsupported dashboard scope.");
            }

            var result = await handler.HandleAsync(request, cancellationToken);

            _logger.Here().MethodExited();
            return result;
        }
    }
}
