using ZZLD_Form.Core.Models;

namespace ZZLD_Form.Infrastructure.Pdf;

/// <summary>
/// Service for PDF generation and processing
/// </summary>
public interface IPdfProcessor
{
    /// <summary>
    /// Generates a ZZLD form PDF from personal data
    /// </summary>
    /// <param name="personalData">Personal data to fill in the form</param>
    /// <param name="templatePath">Path or identifier of the template</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF content as byte array</returns>
    Task<byte[]> GeneratePdfAsync(
        PersonalData personalData,
        string templatePath,
        CancellationToken cancellationToken = default);
}
