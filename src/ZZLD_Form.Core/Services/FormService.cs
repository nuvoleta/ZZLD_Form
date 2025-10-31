using FluentValidation;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.Core.Services;

/// <summary>
/// Implementation of form generation service
/// </summary>
public class FormService : IFormService
{
    private readonly IValidator<PersonalData> _validator;
    private readonly IPdfProcessor _pdfProcessor;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ITemplateService _templateService;

    public FormService(
        IValidator<PersonalData> validator,
        IPdfProcessor pdfProcessor,
        IBlobStorageService blobStorageService,
        ITemplateService templateService)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _pdfProcessor = pdfProcessor ?? throw new ArgumentNullException(nameof(pdfProcessor));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    }

    /// <inheritdoc/>
    public async Task<FormGenerationResult> GenerateFormAsync(
        FormGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Map request to domain model
            var personalData = MapToPersonalData(request);

            // Validate
            var validationResult = await _validator.ValidateAsync(personalData, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new FormGenerationResult
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            // Get template path
            var templatePath = await _templateService.GetTemplatePathAsync(cancellationToken);

            // Generate PDF
            var pdfBytes = await _pdfProcessor.GeneratePdfAsync(personalData, templatePath, cancellationToken);

            // Create metadata
            var formId = GenerateFormId();
            var metadata = new FormMetadata
            {
                FormId = formId,
                FullName = personalData.GetFullName(),
                GeneratedAt = DateTime.UtcNow,
                EGN = personalData.EGN,
                Email = personalData.Email
            };

            // Upload to blob storage
            var (blobName, downloadUrl, expiresAt) = await _blobStorageService.UploadFormAsync(
                pdfBytes,
                metadata,
                cancellationToken);

            return new FormGenerationResult
            {
                Success = true,
                FormId = formId,
                DownloadUrl = downloadUrl,
                BlobName = blobName,
                GeneratedAt = metadata.GeneratedAt,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            return new FormGenerationResult
            {
                Success = false,
                ErrorMessage = $"Form generation failed: {ex.Message}"
            };
        }
    }

    /// <inheritdoc/>
    public async Task<FormGenerationResult> GetFormAsync(string formId, CancellationToken cancellationToken = default)
    {
        try
        {
            var (blobName, downloadUrl, expiresAt, metadata) = await _blobStorageService.GetFormByIdAsync(
                formId,
                cancellationToken);

            return new FormGenerationResult
            {
                Success = true,
                FormId = formId,
                DownloadUrl = downloadUrl,
                BlobName = blobName,
                GeneratedAt = metadata.GeneratedAt,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            return new FormGenerationResult
            {
                Success = false,
                ErrorMessage = $"Failed to retrieve form: {ex.Message}"
            };
        }
    }

    private static PersonalData MapToPersonalData(FormGenerationRequest request)
    {
        return new PersonalData
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            EGN = request.EGN,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            DocumentNumber = request.DocumentNumber,
            DocumentIssueDate = request.DocumentIssueDate,
            DocumentIssuedBy = request.DocumentIssuedBy
        };
    }

    private static string GenerateFormId()
    {
        return $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
    }
}

/// <summary>
/// Interface for PDF processing
/// </summary>
public interface IPdfProcessor
{
    /// <summary>
    /// Generates a PDF from personal data
    /// </summary>
    Task<byte[]> GeneratePdfAsync(PersonalData personalData, string templatePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for blob storage operations
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a form to blob storage
    /// </summary>
    Task<(string BlobName, string DownloadUrl, DateTime ExpiresAt)> UploadFormAsync(
        byte[] pdfBytes,
        FormMetadata metadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a form by its ID
    /// </summary>
    Task<(string BlobName, string DownloadUrl, DateTime ExpiresAt, FormMetadata Metadata)> GetFormByIdAsync(
        string formId,
        CancellationToken cancellationToken = default);
}
