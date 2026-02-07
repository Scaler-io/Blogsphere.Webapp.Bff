using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Features.ApiRoute.Queries.GetApiRouteById;
using Blogsphere.Webapp.Bff.Application.Services;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using FluentAssertions;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Features.ApiRoutes;

public class GetApiRouteByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_success_and_enriches_metadata_users()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var apiGatewayProvider = new Mock<IApiGatewayProvider>();
        var userApiProvider = new Mock<IUserApiProvider>();
        var mapper = TestHelpers.Mapper;

        var routeId = "route-1";
        var requestInfo = TestHelpers.CreateRequestInformation("corr-1");

        var apiRoute = TestData.ApiRoute(id: routeId, createdBy: "creator-id", updatedBy: "updater-id");

        apiGatewayProvider.Setup(x => x.GetApiRouteByIdAsync(routeId, requestInfo, It.IsAny<CancellationToken>())).ReturnsAsync(Result<ApiRoute>.Success(apiRoute));

        userApiProvider
                .Setup(p => p.GetManagementUserNameDetailsById("creator-id", requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ManagementUserDetails>.Success(
                    TestData.ManagementUser("creator-id", "Creator Name", email: "creator@local", employeeId: "E1", jobTitle: "Creator")));

        userApiProvider
            .Setup(p => p.GetManagementUserNameDetailsById("updater-id", requestInfo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ManagementUserDetails>.Success(
                TestData.ManagementUser("updater-id", "Updater Name", email: "updater@local", employeeId: "E2", jobTitle: "Updater")));

        var metaDataUserEnricher = new MetaDataUserEnricher(logger.Object, userApiProvider.Object, mapper);

        var handler = new GetApiRouteByIdQueryHandler(
            logger.Object,
            apiGatewayProvider.Object,
            metaDataUserEnricher,
            mapper);

        var result = await handler.Handle(new GetApiRouteByIdQuery(routeId, requestInfo), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(routeId);
        result.Data.Metadata.Should().NotBeNull();
        result.Data.Metadata.CreatedBy.Should().NotBeNull();
        result.Data.Metadata.CreatedBy.FullName.Should().Be("Creator Name");
        result.Data.Metadata.UpdatedBy.Should().NotBeNull();
        result.Data.Metadata.UpdatedBy.FullName.Should().Be("Updater Name");
    }
}
