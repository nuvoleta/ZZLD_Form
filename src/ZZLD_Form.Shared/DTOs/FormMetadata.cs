namespace ZZLD_Form.Shared.DTOs;

/// <summary>
/// Metadata associated with a generated form
/// </summary>
public class FormMetadata
{
    /// <summary>
    /// Unique form identifier
    /// </summary>
    public string FormId { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the person
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// When the form was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// EGN of the person
    /// </summary>
    public string EGN { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Content type of the blob
    /// </summary>
    public string ContentType { get; set; } = "application/pdf";
}
