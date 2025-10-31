using FluentAssertions;
using FluentValidation.TestHelper;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Core.Validators;

namespace ZZLD_Form.UnitTests.Validators;

public class PersonalDataValidatorTests
{
    private readonly PersonalDataValidator _validator;

    public PersonalDataValidatorTests()
    {
        _validator = new PersonalDataValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var personalData = new PersonalData
        {
            FirstName = "Иван",
            MiddleName = "Петров",
            LastName = "Иванов",
            EGN = "1234567890",
            DateOfBirth = new DateTime(1990, 5, 15),
            Address = "ул. Витоша 10",
            City = "София",
            PostalCode = "1000",
            PhoneNumber = "+359888123456",
            Email = "ivan.ivanov@example.com",
            DocumentNumber = "123456789",
            DocumentIssueDate = new DateTime(2020, 1, 1),
            DocumentIssuedBy = "МВР София"
        };

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithEmptyFirstName_ShouldHaveError(string firstName)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.FirstName = firstName;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithEmptyLastName_ShouldHaveError(string lastName)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.LastName = lastName;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("123abc7890")]
    [InlineData("")]
    public void Validate_WithInvalidEGN_ShouldHaveError(string egn)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.EGN = egn;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EGN);
    }

    [Fact]
    public void Validate_WithValidEGN_ShouldNotHaveError()
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.EGN = "9005150123";

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EGN);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("abcd")]
    [InlineData("")]
    public void Validate_WithInvalidPostalCode_ShouldHaveError(string postalCode)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.PostalCode = postalCode;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PostalCode);
    }

    [Theory]
    [InlineData("1000")]
    [InlineData("1234")]
    [InlineData("9999")]
    public void Validate_WithValidPostalCode_ShouldNotHaveError(string postalCode)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.PostalCode = postalCode;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PostalCode);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.Email = email;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@example.co.uk")]
    [InlineData("ivan.ivanov@gmail.com")]
    public void Validate_WithValidEmail_ShouldNotHaveError(string email)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.Email = email;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithFutureDateOfBirth_ShouldHaveError()
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.DateOfBirth = DateTime.Now.AddDays(1);

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_WithTooOldDateOfBirth_ShouldHaveError()
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.DateOfBirth = DateTime.Now.AddYears(-151);

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_WithFutureDocumentIssueDate_ShouldHaveError()
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.DocumentIssueDate = DateTime.Now.AddDays(1);

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentIssueDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithEmptyAddress_ShouldHaveError(string address)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.Address = address;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithEmptyCity_ShouldHaveError(string city)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.City = city;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithEmptyDocumentNumber_ShouldHaveError(string documentNumber)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.DocumentNumber = documentNumber;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Validate_WithEmptyDocumentIssuedBy_ShouldHaveError(string documentIssuedBy)
    {
        // Arrange
        var personalData = CreateValidPersonalData();
        personalData.DocumentIssuedBy = documentIssuedBy;

        // Act
        var result = _validator.TestValidate(personalData);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DocumentIssuedBy);
    }

    private static PersonalData CreateValidPersonalData()
    {
        return new PersonalData
        {
            FirstName = "Иван",
            MiddleName = "Петров",
            LastName = "Иванов",
            EGN = "1234567890",
            DateOfBirth = new DateTime(1990, 5, 15),
            Address = "ул. Витоша 10",
            City = "София",
            PostalCode = "1000",
            PhoneNumber = "+359888123456",
            Email = "ivan.ivanov@example.com",
            DocumentNumber = "123456789",
            DocumentIssueDate = new DateTime(2020, 1, 1),
            DocumentIssuedBy = "МВР София"
        };
    }
}
