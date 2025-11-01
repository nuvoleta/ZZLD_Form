namespace ZZLD_Form.Shared.DTOs;

public class FormGenerationRequest
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

    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; } = null;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? DocumentIssueDate { get; set; } = null;
    public string DocumentIssuedBy { get; set; } = string.Empty;
}
