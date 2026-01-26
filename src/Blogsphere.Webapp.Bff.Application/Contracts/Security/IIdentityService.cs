using Blogsphere.Webapp.Bff.Domain.Models.Dtos;

namespace Blogsphere.Webapp.Bff.Application.Contracts.Security
{
    public interface IIdentityService
    {
        UserDto PrepareUser();
    }
}