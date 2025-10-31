using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ZZLD_Form.Infrastructure.Storage;
using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.E2ETests;

/// <summary>
/// End-to-end tests for complete form generation workflow
/// </summary>
public class FormGenerationWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public FormGenerationWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteWorkflow_GenerateAndRetrieveForm_Success()
    {
        // Arrange - Mock blob storage for E2E test
        var mockBlobStorage = new Mock<IBlobStorageService>();
        var testFormId = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
        var testBlobName = $"generated/{testFormId}.pdf";
        var testDownloadUrl = "https://test.blob.core.windows.net/test.pdf?sas=token";
        var testExpiresAt = DateTime.UtcNow.AddHours(24);

        mockBlobStorage
            .Setup(x => x.UploadFormAsync(
                It.IsAny<byte[]>(),
                It.IsAny<FormMetadata>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((testBlobName, testDownloadUrl, testExpiresAt));

        mockBlobStorage
            .Setup(x => x.GetFormByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string formId, CancellationToken ct) =>
            {
                var metadata = new FormMetadata
                {
                    FormId = formId,
                    FullName = "Иван Петров Иванов",
                    GeneratedAt = DateTime.UtcNow.AddMinutes(-5),
                    EGN = "1234567890",
                    Email = "ivan.ivanov@example.com"
                };
                return (testBlobName, testDownloadUrl, testExpiresAt, metadata);
            });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove real blob storage service
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBlobStorageService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add mock
                services.AddScoped(_ => mockBlobStorage.Object);
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

        // Act - Step 1: Generate form
        var generateResponse = await client.PostAsJsonAsync("/api/form/generate", request);

        // Assert - Step 1
        generateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var generateResult = await generateResponse.Content.ReadFromJsonAsync<FormGenerationResult>();
        generateResult.Should().NotBeNull();
        generateResult!.Success.Should().BeTrue();
        generateResult.FormId.Should().NotBeNullOrEmpty();
        generateResult.DownloadUrl.Should().NotBeNullOrEmpty();
        generateResult.BlobName.Should().NotBeNullOrEmpty();
        generateResult.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Act - Step 2: Retrieve the generated form
        var formId = generateResult.FormId;
        var retrieveResponse = await client.GetAsync($"/api/form/{formId}");

        // Assert - Step 2
        retrieveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieveResult = await retrieveResponse.Content.ReadFromJsonAsync<FormGenerationResult>();
        retrieveResult.Should().NotBeNull();
        retrieveResult!.Success.Should().BeTrue();
        retrieveResult.FormId.Should().Be(formId);
        retrieveResult.DownloadUrl.Should().NotBeNullOrEmpty();
        retrieveResult.BlobName.Should().NotBeNullOrEmpty();

        // Verify blob storage was called correctly
        mockBlobStorage.Verify(
            x => x.UploadFormAsync(
                It.IsAny<byte[]>(),
                It.Is<FormMetadata>(m => m.FormId == generateResult.FormId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        mockBlobStorage.Verify(
            x => x.GetFormByIdAsync(formId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteWorkflow_WithInvalidData_FailsValidation()
    {
        // Arrange
        var client = _factory.CreateClient();

        var invalidRequest = new FormGenerationRequest
        {
            FirstName = "", // Empty - invalid
            MiddleName = "Петров",
            LastName = "Иванов",
            EGN = "123", // Too short - invalid
            DateOfBirth = DateTime.Now.AddYears(1), // Future date - invalid
            Address = "ул. Витоша 10",
            City = "София",
            PostalCode = "12345", // Too long - invalid
            PhoneNumber = "+359888123456",
            Email = "not-an-email", // Invalid email
            DocumentNumber = "",
            DocumentIssueDate = new DateTime(2020, 1, 1),
            DocumentIssuedBy = "МВР София"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/form/generate", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteWorkflow_RetrieveNonExistentForm_ReturnsNotFound()
    {
        // Arrange
        var mockBlobStorage = new Mock<IBlobStorageService>();
        mockBlobStorage
            .Setup(x => x.GetFormByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Form not found"));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBlobStorageService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockBlobStorage.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/form/non-existent-form-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteWorkflow_HealthCheck_ReturnsHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CompleteWorkflow_WithCyrillicCharacters_GeneratesSuccessfully()
    {
        // Arrange
        var mockBlobStorage = new Mock<IBlobStorageService>();
        mockBlobStorage
            .Setup(x => x.UploadFormAsync(
                It.IsAny<byte[]>(),
                It.IsAny<FormMetadata>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(("generated/test.pdf", "https://test.blob.core.windows.net/test.pdf", DateTime.UtcNow.AddHours(24)));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IBlobStorageService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockBlobStorage.Object);
            });
        }).CreateClient();

        var request = new FormGenerationRequest
        {
            FirstName = "Георги",
            MiddleName = "Димитров",
            LastName = "Петров",
            EGN = "9012345678",
            DateOfBirth = new DateTime(1990, 12, 31),
            Address = "бул. България 25А",
            City = "Пловдив",
            PostalCode = "4000",
            PhoneNumber = "+359877654321",
            Email = "georgi.petrov@test.bg",
            DocumentNumber = "987654321",
            DocumentIssueDate = new DateTime(2021, 6, 15),
            DocumentIssuedBy = "МВР Пловдив"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/form/generate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FormGenerationResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }
}
