namespace ZZLD_Form.Core.Services;

/// <summary>
/// Service for managing form templates
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Gets the ZZLD form template path
    /// </summary>
    /// <returns>Template file path or identifier</returns>
    Task<string> GetTemplatePathAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the template exists and is accessible
    /// </summary>
    /// <returns>True if template is valid</returns>
    Task<bool> ValidateTemplateAsync(CancellationToken cancellationToken = default);
}
