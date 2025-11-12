using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using CustomerManagement.Tests.Utilities;

namespace CustomerManagement.Tests
{
    public class CustomerApiTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public CustomerApiTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().Contain("Healthy");
        }

        [Fact]
        public async Task GetCustomers_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/customers");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}