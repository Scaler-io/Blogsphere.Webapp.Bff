using System.Net;
using FluentAssertions;
using Blogsphere.Webapp.Bff.API.IntegrationTests.Infrastructure;

namespace Blogsphere.Webapp.Bff.API.IntegrationTests.Endpoints
{
    public class TestControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetTest_returns_200_when_authorized()
        {
            // Ensure IdentityService can read a token value.
            _client.DefaultRequestHeaders.Authorization = new("Bearer", "test-token");

            var response = await _client.GetAsync("/api/v1/test");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
