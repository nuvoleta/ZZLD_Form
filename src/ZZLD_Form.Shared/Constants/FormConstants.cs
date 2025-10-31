namespace ZZLD_Form.Shared.Constants;

/// <summary>
/// Constants for ZZLD form processing
/// </summary>
public static class FormConstants
{
    /// <summary>
    /// Expected length of Bulgarian EGN (national identification number)
    /// </summary>
    public const int EgnLength = 10;

    /// <summary>
    /// Expected length of Bulgarian postal code
    /// </summary>
    public const int PostalCodeLength = 4;

    /// <summary>
    /// Date format used in Bulgarian official documents
    /// </summary>
    public const string BulgarianDateFormat = "dd.MM.yyyy";

    /// <summary>
    /// Maximum age for personal data validity (years)
    /// </summary>
    public const int MaxAge = 150;

    /// <summary>
    /// Minimum age for form submission (years)
    /// </summary>
    public const int MinAge = 0;

    /// <summary>
    /// SAS token validity duration (hours)
    /// </summary>
    public const int SasTokenValidityHours = 24;

    /// <summary>
    /// Generated files path prefix
    /// </summary>
    public const string GeneratedFilesPath = "generated";

    /// <summary>
    /// PDF file extension
    /// </summary>
    public const string PdfExtension = ".pdf";

    /// <summary>
    /// Default retry count for blob operations
    /// </summary>
    public const int DefaultRetryCount = 3;

    /// <summary>
    /// Default retry delay in milliseconds
    /// </summary>
    public const int DefaultRetryDelayMs = 1000;
}
