using AutoMapper;
using Blogsphere.Webapp.Bff.Domain.Entities;
using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Mappers
{
    public class EntityToDtoMapper : Profile
    {
        public EntityToDtoMapper()
        {
            CreateMap<ApiCluster, ApiClusterDto>();
            CreateMap<Destination, DestinationDto>();
            CreateMap<ApiRoute, ClusterRouteDto>();

            CreateMap<MetaData, MetaDataDto>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // We will map the created by and updated by manually in the next step
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore()); // We will map the created by and updated by manually in the next step

            CreateMap<ManagementUserDetails, OperationaUserDetailsDto>();
        }
    }
}
