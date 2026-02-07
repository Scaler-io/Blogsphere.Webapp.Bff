using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Services;
using Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Entities.ApiGateway;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using FluentAssertions;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Features.ApiClusters
{
    public class GetApiClusterByIdQueryHandlerTests
    {
        [Fact]
        public async Task Handle_returns_success_and_enriches_metadata_users()
        {
            var logger = TestHelpers.CreateLoggerMock();
            var apiGatewayProvider = new Mock<IApiGatewayProvider>();
            var userApiProvider = new Mock<IUserApiProvider>();
            var mapper = TestHelpers.Mapper;

            var clusterId = "cluster-1";
            var requestInfo = TestHelpers.CreateRequestInformation("corr-1");

            var apiCluster = TestData.ApiCluster(id: clusterId, createdBy: "creator-id", updatedBy: "updater-id");

            apiGatewayProvider
                .Setup(p => p.GetApiCLusterDetailsByIdAsync(clusterId, requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ApiCluster>.Success(apiCluster));

            userApiProvider
                .Setup(p => p.GetManagementUserNameDetailsById("creator-id", requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ManagementUserDetails>.Success(
                    TestData.ManagementUser("creator-id", "Creator Name", email: "creator@local", employeeId: "E1", jobTitle: "Creator")));

            userApiProvider
                .Setup(p => p.GetManagementUserNameDetailsById("updater-id", requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ManagementUserDetails>.Success(
                    TestData.ManagementUser("updater-id", "Updater Name", email: "updater@local", employeeId: "E2", jobTitle: "Updater")));

            var metaDataUserEnricher = new MetaDataUserEnricher(logger.Object, userApiProvider.Object, mapper);

            var handler = new GetApiClusterByIdQueryHandler(
                logger.Object,
                apiGatewayProvider.Object,
                metaDataUserEnricher,
                mapper);

            var result = await handler.Handle(new GetApiClusterByIdQuery(clusterId, requestInfo), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(clusterId);
            result.Data.MetaData.Should().NotBeNull();
            result.Data.MetaData.CreatedBy.Should().NotBeNull();
            result.Data.MetaData.CreatedBy.FullName.Should().Be("Creator Name");
            result.Data.MetaData.UpdatedBy.Should().NotBeNull();
            result.Data.MetaData.UpdatedBy.FullName.Should().Be("Updater Name");
        }

        [Fact]
        public async Task Handle_propagates_failure_when_api_gateway_fails()
        {
            var logger = TestHelpers.CreateLoggerMock();
            var apiGatewayProvider = new Mock<IApiGatewayProvider>();
            var userApiProvider = new Mock<IUserApiProvider>();
            var mapper = TestHelpers.Mapper;

            var clusterId = "missing";
            var requestInfo = TestHelpers.CreateRequestInformation("corr-2");

            apiGatewayProvider
                .Setup(p => p.GetApiCLusterDetailsByIdAsync(clusterId, requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ApiCluster>.Failure(ErrorCode.NotFound, "not found"));

            var metaDataUserEnricher = new MetaDataUserEnricher(logger.Object, userApiProvider.Object, mapper);

            var handler = new GetApiClusterByIdQueryHandler(
                logger.Object,
                apiGatewayProvider.Object,
                metaDataUserEnricher,
                mapper);

            var result = await handler.Handle(new GetApiClusterByIdQuery(clusterId, requestInfo), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }
    }
}
