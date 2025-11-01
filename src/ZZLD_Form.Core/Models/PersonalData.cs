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

    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; } = null;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? DocumentIssueDate { get; set; } = null;
    public string DocumentIssuedBy { get; set; } = string.Empty;

    public string GetFullAddress()
    {
        if (!string.IsNullOrEmpty(Address))
        {
            return Address;
        }

        string[] parts = new string[]
        {
            $"гр.(с) {City} {PostalCode}",
            !string.IsNullOrWhiteSpace(Community) ? $"ж.к. {Community}" : string.Empty,
            !string.IsNullOrWhiteSpace(Street) ? $"ул. {Street} {Number}" : string.Empty,
            !string.IsNullOrWhiteSpace(Block) ? $"бл. {Block}" : string.Empty,
            !string.IsNullOrWhiteSpace(Entrance) ? $"вх. {Entrance}" : string.Empty,
            !string.IsNullOrWhiteSpace(Floor) ? $"ет. {Floor}" : string.Empty,
            !string.IsNullOrWhiteSpace(Apartment) ? $"ап. {Apartment}" : string.Empty
        };
        return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
