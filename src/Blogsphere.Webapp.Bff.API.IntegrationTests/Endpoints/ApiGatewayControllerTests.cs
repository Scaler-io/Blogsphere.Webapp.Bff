using System.Net;
using Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Blogsphere.Webapp.Bff.API.IntegrationTests.Endpoints
{
    public class ApiGatewayControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetApiClusterById_returns_200_and_api_cluster_payload()
        {
            var response = await _client.GetAsync("/api/v2/apigateway/cluster/1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GetApiRouteById_returns_200_and_api_route_payload()
        {
            var response = await _client.GetAsync("/api/v2/apigateway/route/1");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrWhiteSpace();
        }
    }
}
