using FluentValidation;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Shared.Constants;

namespace ZZLD_Form.Core.Validators;

/// <summary>
/// Validator for PersonalData model
/// </summary>
public class PersonalDataValidator : AbstractValidator<PersonalData>
{
    public PersonalDataValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required");

        RuleFor(x => x.EGN)
            .NotEmpty()
            .WithMessage("EGN is required")
            .Length(FormConstants.EgnLength)
            .WithMessage($"EGN must be exactly {FormConstants.EgnLength} digits")
            .Matches(@"^\d{10}$")
            .WithMessage("EGN must contain only digits");


        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required");

        RuleFor(x => x.PostalCode)
            .NotEmpty()
            .WithMessage("Postal code is required")
            .Length(FormConstants.PostalCodeLength)
            .WithMessage($"Postal code must be exactly {FormConstants.PostalCodeLength} digits")
            .Matches(@"^\d{4}$")
            .WithMessage("Postal code must contain only digits");

    }
}
