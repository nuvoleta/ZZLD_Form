using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ZZLD_Form.API.Controllers;

namespace ZZLD_Form.IntegrationTests.Controllers;

public class HealthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var healthStatus = await response.Content.ReadFromJsonAsync<HealthStatus>();
        healthStatus.Should().NotBeNull();
        healthStatus!.Status.Should().Be("Healthy");
        healthStatus.Version.Should().NotBeNullOrEmpty();
    }
}
