using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Services;
using Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Entities;
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

            var apiCluster = new ApiCluster
            {
                Id = clusterId,
                ClusterId = "c1",
                Destinations = [],
                Routes = [],
                MetaData = new MetaData
                {
                    CreatedBy = "creator-id",
                    UpdatedBy = "updater-id",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            apiGatewayProvider
                .Setup(p => p.GetApiCLusterDetailsByIdAsync(clusterId, requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ApiCluster>.Success(apiCluster));

            userApiProvider
                .Setup(p => p.GetManagementUserNameDetailsById("creator-id", requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ManagementUserDetails>.Success(new ManagementUserDetails
                {
                    Id = "creator-id",
                    FullName = "Creator Name",
                    Email = "creator@local",
                    EmployeeId = "E1",
                    JobTitle = "Creator"
                }));

            userApiProvider
                .Setup(p => p.GetManagementUserNameDetailsById("updater-id", requestInfo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ManagementUserDetails>.Success(new ManagementUserDetails
                {
                    Id = "updater-id",
                    FullName = "Updater Name",
                    Email = "updater@local",
                    EmployeeId = "E2",
                    JobTitle = "Updater"
                }));

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
