using Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Analytics;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Scopes;
using Blogsphere.Webapp.Bff.Application.Mappers;
using Blogsphere.Webapp.Bff.Application.Contracts.Enrichment;
using Blogsphere.Webapp.Bff.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Blogsphere.Webapp.Bff.Application.DI
{
    public static class BusinessLogicServiceExtensions
    {
        public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(EntityToDtoMapper).Assembly);
            services.AddMediatR(typeof(GetApiClusterByIdQuery).Assembly);
            services.AddScoped<IDashboardScopeHandler, ApiManagementDashboardScopeHandler>();
            services.AddScoped<IDashboardScopeHandler, UserManagementDashboardScopeHandler>();
            services.AddScoped<IUserManagementDashboardAnalyticsContributor, ManagementUserDashboardAnalyticsContributor>();
            services.AddScoped<IMetaDataUserEnricher, MetaDataUserEnricher>();
            return services;
        }
    }
}