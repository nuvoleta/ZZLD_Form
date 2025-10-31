using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Core.Services;
using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.UnitTests.Services;

public class FormServiceTests
{
    private readonly Mock<IValidator<PersonalData>> _validatorMock;
    private readonly Mock<IPdfProcessor> _pdfProcessorMock;
    private readonly Mock<IBlobStorageService> _blobStorageMock;
    private readonly Mock<ITemplateService> _templateServiceMock;
    private readonly FormService _formService;

    public FormServiceTests()
    {
        _validatorMock = new Mock<IValidator<PersonalData>>();
        _pdfProcessorMock = new Mock<IPdfProcessor>();
        _blobStorageMock = new Mock<IBlobStorageService>();
        _templateServiceMock = new Mock<ITemplateService>();

        _formService = new FormService(
            _validatorMock.Object,
            _pdfProcessorMock.Object,
            _blobStorageMock.Object,
            _templateServiceMock.Object
        );
    }

    [Fact]
    public async Task GenerateFormAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = CreateValidRequest();
        var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };
        var blobName = "generated/test.pdf";
        var downloadUrl = "https://storage.blob.core.windows.net/container/test.pdf?sas=token";
        var expiresAt = DateTime.UtcNow.AddHours(24);

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<PersonalData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _templateServiceMock
            .Setup(x => x.GetTemplatePathAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("template.pdf");

        _pdfProcessorMock
            .Setup(x => x.GeneratePdfAsync(It.IsAny<PersonalData>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pdfBytes);

        _blobStorageMock
            .Setup(x => x.UploadFormAsync(
                It.IsAny<byte[]>(),
                It.IsAny<FormMetadata>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((blobName, downloadUrl, expiresAt));

        // Act
        var result = await _formService.GenerateFormAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FormId.Should().NotBeNullOrEmpty();
        result.DownloadUrl.Should().Be(downloadUrl);
        result.BlobName.Should().Be(blobName);
        result.ExpiresAt.Should().Be(expiresAt);
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ErrorMessage.Should().BeNull();

        _validatorMock.Verify(x => x.ValidateAsync(It.IsAny<PersonalData>(), It.IsAny<CancellationToken>()), Times.Once);
        _templateServiceMock.Verify(x => x.GetTemplatePathAsync(It.IsAny<CancellationToken>()), Times.Once);
        _pdfProcessorMock.Verify(x => x.GeneratePdfAsync(It.IsAny<PersonalData>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _blobStorageMock.Verify(x => x.UploadFormAsync(It.IsAny<byte[]>(), It.IsAny<FormMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateFormAsync_WithInvalidRequest_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateValidRequest();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("FirstName", "First name is required")
        });

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<PersonalData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _formService.GenerateFormAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("First name is required");

        _validatorMock.Verify(x => x.ValidateAsync(It.IsAny<PersonalData>(), It.IsAny<CancellationToken>()), Times.Once);
        _pdfProcessorMock.Verify(x => x.GeneratePdfAsync(It.IsAny<PersonalData>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _blobStorageMock.Verify(x => x.UploadFormAsync(It.IsAny<byte[]>(), It.IsAny<FormMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateFormAsync_WhenPdfGenerationFails_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateValidRequest();

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<PersonalData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _templateServiceMock
            .Setup(x => x.GetTemplatePathAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("template.pdf");

        _pdfProcessorMock
            .Setup(x => x.GeneratePdfAsync(It.IsAny<PersonalData>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("PDF generation failed"));

        // Act
        var result = await _formService.GenerateFormAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("PDF generation failed");

        _blobStorageMock.Verify(x => x.UploadFormAsync(It.IsAny<byte[]>(), It.IsAny<FormMetadata>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateFormAsync_WhenBlobUploadFails_ReturnsFailureResult()
    {
        // Arrange
        var request = CreateValidRequest();
        var pdfBytes = new byte[] { 1, 2, 3, 4, 5 };

        _validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<PersonalData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _templateServiceMock
            .Setup(x => x.GetTemplatePathAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("template.pdf");

        _pdfProcessorMock
            .Setup(x => x.GeneratePdfAsync(It.IsAny<PersonalData>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pdfBytes);

        _blobStorageMock
            .Setup(x => x.UploadFormAsync(It.IsAny<byte[]>(), It.IsAny<FormMetadata>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Blob upload failed"));

        // Act
        var result = await _formService.GenerateFormAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Blob upload failed");
    }

    [Fact]
    public async Task GetFormAsync_WithValidFormId_ReturnsFormResult()
    {
        // Arrange
        var formId = "test-form-id";
        var blobName = "generated/test.pdf";
        var downloadUrl = "https://storage.blob.core.windows.net/container/test.pdf?sas=token";
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var metadata = new FormMetadata
        {
            FormId = formId,
            FullName = "John Doe",
            GeneratedAt = DateTime.UtcNow.AddHours(-1)
        };

        _blobStorageMock
            .Setup(x => x.GetFormByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((blobName, downloadUrl, expiresAt, metadata));

        // Act
        var result = await _formService.GetFormAsync(formId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FormId.Should().Be(formId);
        result.DownloadUrl.Should().Be(downloadUrl);
        result.BlobName.Should().Be(blobName);
        result.ExpiresAt.Should().Be(expiresAt);
        result.GeneratedAt.Should().Be(metadata.GeneratedAt);

        _blobStorageMock.Verify(x => x.GetFormByIdAsync(formId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFormAsync_WhenFormNotFound_ReturnsFailureResult()
    {
        // Arrange
        var formId = "non-existent-form-id";

        _blobStorageMock
            .Setup(x => x.GetFormByIdAsync(formId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("Form not found"));

        // Act
        var result = await _formService.GetFormAsync(formId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Form not found");
    }

    private static FormGenerationRequest CreateValidRequest()
    {
        return new FormGenerationRequest
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
    }
}
