namespace ZZLD_Form.Shared.DTOs;

/// <summary>
/// Result DTO for form generation
/// </summary>
public class FormGenerationResult
{
    /// <summary>
    /// Unique identifier for the generated form
    /// </summary>
    public string FormId { get; set; } = string.Empty;

    /// <summary>
    /// URL to download the generated PDF
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the form was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Expiry date of the download URL (SAS token expiration)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Blob name in storage
    /// </summary>
    public string BlobName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if generation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
