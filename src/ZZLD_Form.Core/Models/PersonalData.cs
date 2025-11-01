namespace ZZLD_Form.Core.Models;

/// <summary>
/// Domain model representing personal data for ZZLD form
/// </summary>
public class PersonalData
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EGN { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Community { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Block { get; set; } = string.Empty;
    public string Entrance { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Apartment { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string GetFullName() => $"{FirstName} {MiddleName} {LastName}".Trim();
}
