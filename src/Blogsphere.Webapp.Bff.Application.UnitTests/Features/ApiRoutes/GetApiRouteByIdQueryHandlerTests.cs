using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Contracts.Enrichment;
using Blogsphere.Webapp.Bff.Application.Features.ApiRoute.Queries.GetApiRouteById;
using Blogsphere.Webapp.Bff.Application.Services;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Entities;
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

        var apiRoute = new ApiRoute
        {
            Id = routeId,
            RouteId = "r1",
            Path = "/route1",
            Methods = ["GET"],
            RateLimiterPolicy = "rate-limiter-policy",
            IsActive = true,
            ClusterId = "cluster-1",
            Headers =
            [
                new ApiRouteHeader { Id = "header-1", Name = "Header 1", Values = ["Value 1"], Mode = "Mode 1", IsActive = true },
            ],
            Transforms =
            [
                new ApiRouteTransform { Id = "transform-1", PathPattern = "Path Pattern 1", IsActive = true },
            ],
            Metadata = new MetaData
            {
                CreatedBy = "creator-id",
                UpdatedBy = "updater-id",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
            },
        };

        apiGatewayProvider.Setup(x => x.GetApiRouteByIdAsync(routeId, requestInfo, It.IsAny<CancellationToken>())).ReturnsAsync(Result<ApiRoute>.Success(apiRoute));

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
