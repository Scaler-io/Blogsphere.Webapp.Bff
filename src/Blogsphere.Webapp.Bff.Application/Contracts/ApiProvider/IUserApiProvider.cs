using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using System.Threading;

namespace Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider
{
    public interface IUserApiProvider
    {
        Task<Result<ManagementUserDetails>> GetManagementUserNameDetailsById(
            string id,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default);
    }
}
