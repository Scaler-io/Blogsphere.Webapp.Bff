using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Core;

namespace Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure.Fakes
{
    public sealed class FakeUserApiProvider : IUserApiProvider
    {
        public Task<Result<ManagementUserDetails>> GetManagementUserNameDetailsById(
            string id,
            RequestInformation requestInformation,
            CancellationToken cancellationToken = default)
        {
            var user = new ManagementUserDetails
            {
                Id = id,
                FullName = $"Test User ({id})",
                Email = $"{id}@local",
                EmployeeId = "E-TEST",
                JobTitle = "Test",
            };

            return Task.FromResult(Result<ManagementUserDetails>.Success(user));
        }
    }
}
