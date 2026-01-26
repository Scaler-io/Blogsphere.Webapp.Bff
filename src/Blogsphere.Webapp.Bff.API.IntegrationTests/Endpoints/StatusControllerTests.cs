using System.Net;
using FluentAssertions;
using Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure;

namespace Blogsphere.Webapp.Bff.API.IntegrationTests.Endpoints
{
    public class StatusControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetStatus_returns_200_and_status_payload()
        {
            var response = await _client.GetAsync("/api/v1/status");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrWhiteSpace();
        }
    }
}
