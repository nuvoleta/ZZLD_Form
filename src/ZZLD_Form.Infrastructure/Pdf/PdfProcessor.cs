using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Core.Services;
using ZZLD_Form.Shared.Constants;
using Microsoft.Extensions.Logging;

namespace ZZLD_Form.Infrastructure.Pdf;

public class PdfProcessor : IPdfProcessor
{
    private readonly ILogger<PdfProcessor>? _logger;

    public PdfProcessor(ILogger<PdfProcessor>? logger = null)
    {
        _logger = logger;
    }
    public async Task<byte[]> GeneratePdfAsync(
        PersonalData personalData,
        string templatePath,
        CancellationToken cancellationToken = default)
    {
        if (personalData == null)
            throw new ArgumentNullException(nameof(personalData));

        if (string.IsNullOrWhiteSpace(templatePath) || !File.Exists(templatePath))
            throw new FileNotFoundException($"Template file not found at: {templatePath}");

        return await Task.Run(() =>
        {
            try
            {
                var outputStream = new MemoryStream();
                
                using (var reader = new PdfReader(templatePath))
                using (var writer = new PdfWriter(outputStream))
                {
                    writer.SetCloseStream(false);
                    
                    using (var pdfDoc = new PdfDocument(reader, writer))
                    {
                        var page = pdfDoc.GetPage(1);
                        var pageSize = page.GetPageSize();
                        
                        var canvas = new PdfCanvas(page);
                        
                        string fontPath = "/mnt/c/Windows/Fonts/arialbd.ttf";
                        var font = PdfFontFactory.CreateFont(fontPath, iText.IO.Font.PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                        
                        canvas.SetFillColor(ColorConstants.BLACK);
                        float fontSize = 10f;
                        
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(90, 640).ShowText(personalData.FirstName).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(250, 640).ShowText(personalData.MiddleName).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(425, 640).ShowText(personalData.LastName).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(110, 618).ShowText(personalData.EGN).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(125, 605).ShowText(personalData.City).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(425, 605).ShowText(personalData.PostalCode).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(110, 592).ShowText(personalData.Community).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(207, 592).ShowText(personalData.Street).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(325, 592).ShowText(personalData.Number).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(362, 592).ShowText(personalData.Block).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(460, 592).ShowText(personalData.Entrance).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(510, 592).ShowText(personalData.Floor).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(110, 580).ShowText(personalData.Apartment).EndText();
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(160, 580).ShowText(personalData.PhoneNumber).EndText();
                    }
                }
                
                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process PDF: {ex.GetType().Name} - {ex.Message}", ex);
            }
        }, cancellationToken);
    }
}
