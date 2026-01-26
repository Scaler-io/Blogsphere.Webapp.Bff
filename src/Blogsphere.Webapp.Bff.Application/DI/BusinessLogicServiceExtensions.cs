using Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById;
using Blogsphere.Webapp.Bff.Application.Mappers;
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
            return services;
        }
    }
}