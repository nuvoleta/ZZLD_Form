using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ZZLD_Form.Core.Services;
using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.IntegrationTests.Controllers;

public class FormControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FormControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GenerateForm_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var mockFormService = new Mock<IFormService>();
        mockFormService
            .Setup(x => x.GenerateFormAsync(It.IsAny<FormGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormGenerationResult
            {
                Success = true,
                FormId = "test-form-id",
                DownloadUrl = "https://test.blob.core.windows.net/test.pdf",
                BlobName = "generated/test.pdf",
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFormService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add mock
                services.AddScoped(_ => mockFormService.Object);
            });
        }).CreateClient();

        var request = new FormGenerationRequest
        {
            FirstName = "Иван",
            MiddleName = "Петров",
            LastName = "Иванов",
            EGN = "1234567890",
            DateOfBirth = new DateTime(1990, 5, 15),
            Address = "ул. Витоша 10",
            City = "София",
            PostalCode = "1000",
            PhoneNumber = "+359888123456",
            Email = "ivan.ivanov@example.com",
            DocumentNumber = "123456789",
            DocumentIssueDate = new DateTime(2020, 1, 1),
            DocumentIssuedBy = "МВР София"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/form/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FormGenerationResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.FormId.Should().Be("test-form-id");
    }

    [Fact]
    public async Task GenerateForm_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var mockFormService = new Mock<IFormService>();
        mockFormService
            .Setup(x => x.GenerateFormAsync(It.IsAny<FormGenerationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormGenerationResult
            {
                Success = false,
                ErrorMessage = "Validation failed"
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFormService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockFormService.Object);
            });
        }).CreateClient();

        var request = new FormGenerationRequest
        {
            FirstName = "", // Invalid
            LastName = "Test",
            EGN = "123", // Invalid
            Email = "invalid-email"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/form/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetForm_WithValidFormId_ReturnsOk()
    {
        // Arrange
        var formId = "test-form-id";
        var mockFormService = new Mock<IFormService>();
        mockFormService
            .Setup(x => x.GetFormAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormGenerationResult
            {
                Success = true,
                FormId = formId,
                DownloadUrl = "https://test.blob.core.windows.net/test.pdf",
                BlobName = "generated/test.pdf",
                GeneratedAt = DateTime.UtcNow.AddHours(-1),
                ExpiresAt = DateTime.UtcNow.AddHours(23)
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFormService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockFormService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync($"/api/form/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FormGenerationResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.FormId.Should().Be(formId);
    }

    [Fact]
    public async Task GetForm_WithNonExistentFormId_ReturnsNotFound()
    {
        // Arrange
        var formId = "non-existent-form-id";
        var mockFormService = new Mock<IFormService>();
        mockFormService
            .Setup(x => x.GetFormAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FormGenerationResult
            {
                Success = false,
                ErrorMessage = "Form not found"
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFormService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockFormService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync($"/api/form/{formId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
