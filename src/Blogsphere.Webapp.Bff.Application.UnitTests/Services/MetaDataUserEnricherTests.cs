using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using Blogsphere.Webapp.Bff.Application.Services;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Entities;
using Blogsphere.Webapp.Bff.Domain.Entities.ManagementUsers;
using Blogsphere.Webapp.Bff.Domain.Models.Constants;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using FluentAssertions;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Services;

public class MetaDataUserEnricherTests
{
    [Fact]
    public async Task EnrichAsync_when_target_is_null_returns_internal_error()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var userApi = new Mock<IUserApiProvider>(MockBehavior.Strict);
        var enricher = new MetaDataUserEnricher(logger.Object, userApi.Object, TestHelpers.Mapper);
        var requestInfo = TestHelpers.CreateRequestInformation();

        var result = await enricher.EnrichAsync(TestData.MetaData(), null!, requestInfo, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.InternalServerError);
        result.ErrorMessage.Should().Be(ErrorMessages.InternalServerError);
    }

    [Fact]
    public async Task EnrichAsync_when_no_user_ids_skips_provider_and_returns_success()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var userApi = new Mock<IUserApiProvider>(MockBehavior.Strict);
        var enricher = new MetaDataUserEnricher(logger.Object, userApi.Object, TestHelpers.Mapper);

        var target = new MetaDataDto();
        var source = new MetaData { CreatedBy = null, UpdatedBy = null, CreatedAt = default, UpdatedAt = default };
        var requestInfo = TestHelpers.CreateRequestInformation();

        var result = await enricher.EnrichAsync(source, target, requestInfo, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(target);
        userApi.Verify(
            p => p.GetManagementUserNameDetailsById(It.IsAny<string>(), It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EnrichAsync_propagates_failure_when_user_lookup_fails()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var userApi = new Mock<IUserApiProvider>();
        userApi
            .Setup(p => p.GetManagementUserNameDetailsById("u1", It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ManagementUserDetails>.Failure(ErrorCode.NotFound, ErrorMessages.NotFound));

        var enricher = new MetaDataUserEnricher(logger.Object, userApi.Object, TestHelpers.Mapper);
        var target = new MetaDataDto();
        var source = TestData.MetaData(createdBy: "u1", updatedBy: "u1");
        var requestInfo = TestHelpers.CreateRequestInformation();

        var result = await enricher.EnrichAsync(source, target, requestInfo, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task EnrichAsync_maps_distinct_users_onto_created_and_updated()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var userApi = new Mock<IUserApiProvider>();
        userApi
            .Setup(p => p.GetManagementUserNameDetailsById("creator-id", It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ManagementUserDetails>.Success(
                TestData.ManagementUser("creator-id", "Creator", email: "c@local", employeeId: "E1", jobTitle: "Dev")));
        userApi
            .Setup(p => p.GetManagementUserNameDetailsById("updater-id", It.IsAny<RequestInformation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ManagementUserDetails>.Success(
                TestData.ManagementUser("updater-id", "Updater", email: "u@local", employeeId: "E2", jobTitle: "Lead")));

        var enricher = new MetaDataUserEnricher(logger.Object, userApi.Object, TestHelpers.Mapper);
        var target = new MetaDataDto();
        var source = TestData.MetaData(createdBy: "creator-id", updatedBy: "updater-id");
        var requestInfo = TestHelpers.CreateRequestInformation();

        var result = await enricher.EnrichAsync(source, target, requestInfo, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.CreatedBy!.FullName.Should().Be("Creator");
        result.Data.UpdatedBy!.FullName.Should().Be("Updater");
    }
}
