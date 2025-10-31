namespace ZZLD_Form.Shared.DTOs;

/// <summary>
/// Request DTO for generating a ZZLD form
/// </summary>
public class FormGenerationRequest
{
    /// <summary>
    /// First name of the person
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Middle name of the person
    /// </summary>
    public string MiddleName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the person
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Bulgarian national identification number (EGN) - 10 digits
    /// </summary>
    public string EGN { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Postal code - 4 digits
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
}
