namespace ZZLD_Form.Core.Models;

/// <summary>
/// Domain model representing personal data for ZZLD form
/// </summary>
public class PersonalData
{
    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Middle name
    /// </summary>
    public string MiddleName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Bulgarian national identification number (EGN)
    /// </summary>
    public string EGN { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Full address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Postal code (4 digits)
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Identity document number
    /// </summary>
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Document issue date
    /// </summary>
    public DateTime DocumentIssueDate { get; set; }

    /// <summary>
    /// Document issuing authority
    /// </summary>
    public string DocumentIssuedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets the full name
    /// </summary>
    public string GetFullName() => $"{FirstName} {MiddleName} {LastName}".Trim();
}
