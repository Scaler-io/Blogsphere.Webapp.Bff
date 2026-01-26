using Asp.Versioning;
using Blogsphere.Webapp.Bff.Application.Contracts.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Blogsphere.Webapp.Bff.Application.Extensions;
using Blogsphere.Webapp.Bff.Application.Contracts.ApiProvider;
using MediatR;
using Blogsphere.Webapp.Bff.Application.Features.ApiClusters.Queries.GetApiClusterById;

namespace Blogsphere.Webapp.Bff.API.Controllers.v1
{
    [Authorize]
    [ApiVersion("1")]
    public class TestController(ILogger logger, IIdentityService identityService, IApiGatewayProvider apiGatewayProvider, IMediator mediator) : BaseApiController(logger, identityService)
    {
        // private readonly UserApiProvider _userApiProvider = userApiProvider;
        private readonly IApiGatewayProvider _apiGatewayProvider = apiGatewayProvider;
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            Logger.Here().MethodEntered();
            var result = await _mediator.Send(new GetApiClusterByIdQuery("0c15927e-4453-47d6-b0ab-04658351c4e8", RequestInformation), cancellationToken);
            Logger.Here().MethodExited();
            return OkOrFailure(result);
        }
    }
}