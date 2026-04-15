using Blogsphere.Webapp.Bff.Application.Features.Dashboard;
using Blogsphere.Webapp.Bff.Application.Features.Dashboard.Queries.GetDashboard;
using Blogsphere.Webapp.Bff.Application.UnitTests.Common;
using Blogsphere.Webapp.Bff.Domain.Models.Core;
using Blogsphere.Webapp.Bff.Domain.Models.Dtos.Dashboard;
using Blogsphere.Webapp.Bff.Domain.Models.Enums;
using FluentAssertions;
using Moq;

namespace Blogsphere.Webapp.Bff.Application.UnitTests.Features.Dashboard;

public class GetDashboardQueryHandlerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_when_scope_is_missing_or_whitespace_returns_bad_request(string? scope)
    {
        var logger = TestHelpers.CreateLoggerMock();
        var scopeHandler = new Mock<IDashboardScopeHandler>();
        scopeHandler.Setup(h => h.Scope).Returns(DashboardScopes.ApiManagement);

        var handler = new GetDashboardQueryHandler(logger.Object, [scopeHandler.Object]);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), scope!);

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.BadRequest);
        result.ErrorMessage.Should().Be("The scope query parameter is required.");
        scopeHandler.Verify(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_when_scope_is_unknown_returns_bad_request()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var registered = new Mock<IDashboardScopeHandler>();
        registered.Setup(h => h.Scope).Returns(DashboardScopes.ApiManagement);

        var handler = new GetDashboardQueryHandler(logger.Object, [registered.Object]);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), "unknown-scope");

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.BadRequest);
        result.ErrorMessage.Should().Be("Unsupported dashboard scope.");
        registered.Verify(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_trims_scope_before_lookup()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var scopeHandler = new Mock<IDashboardScopeHandler>();
        scopeHandler.Setup(h => h.Scope).Returns(DashboardScopes.UserManagement);
        var dto = new UserManagementDashboardDto();
        scopeHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DashboardResponseBase>.Success(dto));

        var handler = new GetDashboardQueryHandler(logger.Object, [scopeHandler.Object]);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), $"  {DashboardScopes.UserManagement}  ");

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(dto);
        scopeHandler.Verify(
            h => h.HandleAsync(It.Is<GetDashboardQuery>(q => q.Scope == $"  {DashboardScopes.UserManagement}  "), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_scope_matching_is_case_insensitive()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var scopeHandler = new Mock<IDashboardScopeHandler>();
        scopeHandler.Setup(h => h.Scope).Returns(DashboardScopes.ApiManagement);
        var dto = new ApiManagementDashboardDto();
        scopeHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DashboardResponseBase>.Success(dto));

        var handler = new GetDashboardQueryHandler(logger.Object, [scopeHandler.Object]);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), "API-MANAGEMENT");

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        scopeHandler.Verify(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_delegates_to_the_handler_registered_for_that_scope()
    {
        var logger = TestHelpers.CreateLoggerMock();
        var apiHandler = new Mock<IDashboardScopeHandler>();
        apiHandler.Setup(h => h.Scope).Returns(DashboardScopes.ApiManagement);
        var userHandler = new Mock<IDashboardScopeHandler>();
        userHandler.Setup(h => h.Scope).Returns(DashboardScopes.UserManagement);
        var expected = new UserManagementDashboardDto();
        userHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DashboardResponseBase>.Success(expected));

        var handler = new GetDashboardQueryHandler(logger.Object, [apiHandler.Object, userHandler.Object]);
        var request = new GetDashboardQuery(TestHelpers.CreateRequestInformation(), DashboardScopes.UserManagement);

        var result = await handler.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(expected);
        apiHandler.Verify(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        userHandler.Verify(h => h.HandleAsync(It.IsAny<GetDashboardQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
