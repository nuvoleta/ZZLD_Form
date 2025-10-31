using ZZLD_Form.Shared.DTOs;

namespace ZZLD_Form.Core.Services;

/// <summary>
/// Service for orchestrating form generation workflow
/// </summary>
public interface IFormService
{
    /// <summary>
    /// Generates a ZZLD form PDF and stores it in blob storage
    /// </summary>
    /// <param name="request">Form generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Form generation result with download URL</returns>
    Task<FormGenerationResult> GenerateFormAsync(FormGenerationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a previously generated form by its ID
    /// </summary>
    /// <param name="formId">Form identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Form generation result with download URL</returns>
    Task<FormGenerationResult> GetFormAsync(string formId, CancellationToken cancellationToken = default);
}
