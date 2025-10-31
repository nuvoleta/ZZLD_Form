using FluentAssertions;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Infrastructure.Pdf;

namespace ZZLD_Form.IntegrationTests.Pdf;

public class PdfProcessorTests
{
    private readonly PdfProcessor _pdfProcessor;

    public PdfProcessorTests()
    {
        _pdfProcessor = new PdfProcessor();
    }

    [Fact]
    public async Task GeneratePdfAsync_WithValidData_GeneratesPdf()
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
        var pdfBytes = await _pdfProcessor.GeneratePdfAsync(personalData, "template.pdf");

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Should().NotBeEmpty();
        pdfBytes.Length.Should().BeGreaterThan(100); // PDF should have reasonable size
        
        // Check PDF magic number (PDF files start with %PDF)
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public async Task GeneratePdfAsync_WithCyrillicCharacters_GeneratesPdf()
    {
        // Arrange
        var personalData = new PersonalData
        {
            FirstName = "Георги",
            MiddleName = "Димитров",
            LastName = "Петров",
            EGN = "9012345678",
            DateOfBirth = new DateTime(1990, 12, 31),
            Address = "бул. България 25А",
            City = "Пловдив",
            PostalCode = "4000",
            PhoneNumber = "+359877654321",
            Email = "georgi.petrov@test.bg",
            DocumentNumber = "987654321",
            DocumentIssueDate = new DateTime(2021, 6, 15),
            DocumentIssuedBy = "МВР Пловдив"
        };

        // Act
        var pdfBytes = await _pdfProcessor.GeneratePdfAsync(personalData, "template.pdf");

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Should().NotBeEmpty();
        
        // Verify it's a valid PDF
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4);
        header.Should().Be("%PDF");
    }

    [Fact]
    public async Task GeneratePdfAsync_WithNullPersonalData_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _pdfProcessor.GeneratePdfAsync(null!, "template.pdf"));
    }

    [Fact]
    public async Task GeneratePdfAsync_WithMinimalData_GeneratesPdf()
    {
        // Arrange
        var personalData = new PersonalData
        {
            FirstName = "А",
            MiddleName = "Б",
            LastName = "В",
            EGN = "0000000000",
            DateOfBirth = DateTime.Now.AddYears(-20),
            Address = "ул. А 1",
            City = "София",
            PostalCode = "1000",
            PhoneNumber = "0888000000",
            Email = "a@b.c",
            DocumentNumber = "1",
            DocumentIssueDate = DateTime.Now.AddYears(-1),
            DocumentIssuedBy = "МВР"
        };

        // Act
        var pdfBytes = await _pdfProcessor.GeneratePdfAsync(personalData, "template.pdf");

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Should().NotBeEmpty();
    }
}
