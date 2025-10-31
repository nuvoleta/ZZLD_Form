namespace ZZLD_Form.Core.Services;

/// <summary>
/// Implementation of template service
/// </summary>
public class TemplateService : ITemplateService
{
    private const string DefaultTemplateName = "ZZLD_Form_Template.pdf";

    /// <inheritdoc/>
    public Task<string> GetTemplatePathAsync(CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would fetch from blob storage or file system
        // For now, return a template identifier
        return Task.FromResult(DefaultTemplateName);
    }

    /// <inheritdoc/>
    public Task<bool> ValidateTemplateAsync(CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would check if template exists
        return Task.FromResult(true);
    }
}
