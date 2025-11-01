using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Core.Services;
using ZZLD_Form.Shared.Constants;
using Microsoft.Extensions.Logging;
using System.Globalization;

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
                        
                        //string fontPath = "/mnt/c/Windows/Fonts/arialbd.ttf";
                        //string fontPath = "/Windows/Fonts/arialbd.ttf";
                        string fontPath = "/Windows/Fonts/verdana.ttf";
                        var font = PdfFontFactory.CreateFont(fontPath, iText.IO.Font.PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                        string fontPathBold = "/Windows/Fonts/verdanab.ttf";
                        var fontBold = PdfFontFactory.CreateFont(fontPathBold, iText.IO.Font.PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
                        string fontPathItalic = "/Windows/Fonts/verdanai.ttf";
                        var fontItalic = PdfFontFactory.CreateFont(fontPathItalic, iText.IO.Font.PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

                        canvas.SetFillColor(ColorConstants.BLACK);
                        float fontSize = 10f;
                        int lineHeight = 16;
                        int lineX = 85;
                        int lineY = 654;

                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(lineX, lineY).ShowText("Äîëóïîäïèñàíèÿò/àòà").EndText();
                        lineY -= lineHeight;
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(lineX, lineY - 2).ShowText("…………………………………………………………………………………………………………………………………………………").EndText();
                        canvas.BeginText().SetFontAndSize(fontBold, fontSize).MoveText(lineX, lineY).ShowText(personalData.GetFullName()).EndText();
                        lineY -= lineHeight;
                        canvas.BeginText().SetFontAndSize(fontItalic, fontSize).MoveText(lineX, lineY).ShowText("(òðèòå èìåíà ïî äîêóìåíò çà ñàìîëè÷íîñò)").EndText();
                        lineY -= lineHeight;
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(lineX, lineY).ShowText($"ÅÃÍ: {personalData.EGN}").EndText();
                        lineY -= lineHeight;
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(lineX, lineY).ShowText($"Àäðåñ: {personalData.GetFullAddress()}").EndText();
                        lineY -= lineHeight;
                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(lineX, lineY).ShowText($"Òåëåôîí: {personalData.PhoneNumber}").EndText();

                        canvas.BeginText().SetFontAndSize(font, fontSize).MoveText(lineX + 35, 227).ShowText(DateTime.Today.ToString("d MMMM yyyy ã.", new CultureInfo("bg-BG"))).EndText();
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
