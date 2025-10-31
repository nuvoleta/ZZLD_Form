using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZZLD_Form.Core.Models;
using ZZLD_Form.Shared.Constants;

namespace ZZLD_Form.Infrastructure.Pdf;

/// <summary>
/// PDF processor implementation using QuestPDF
/// Generates ZZLD forms with Bulgarian Cyrillic support
/// </summary>
public class PdfProcessor : IPdfProcessor
{
    static PdfProcessor()
    {
        // Configure QuestPDF license (Community license for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc/>
    public Task<byte[]> GeneratePdfAsync(
        PersonalData personalData,
        string templatePath,
        CancellationToken cancellationToken = default)
    {
        if (personalData == null)
            throw new ArgumentNullException(nameof(personalData));

        return Task.Run(() =>
        {
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .AlignCenter()
                        .Text("ДЕКЛАРАЦИЯ ПО ЗЗЛД")
                        .FontSize(16)
                        .Bold();

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Personal Information Section
                            AddSection(column, "ЛИЧНИ ДАННИ");
                            
                            AddField(column, "Име:", personalData.FirstName);
                            AddField(column, "Презиме:", personalData.MiddleName);
                            AddField(column, "Фамилия:", personalData.LastName);
                            AddField(column, "ЕГН:", personalData.EGN);
                            AddField(column, "Дата на раждане:", personalData.DateOfBirth.ToString(FormConstants.BulgarianDateFormat));

                            // Contact Information Section
                            AddSection(column, "АДРЕС И КОНТАКТИ");
                            
                            AddField(column, "Адрес:", personalData.Address);
                            AddField(column, "Град:", personalData.City);
                            AddField(column, "Пощенски код:", personalData.PostalCode);
                            AddField(column, "Телефон:", personalData.PhoneNumber);
                            AddField(column, "Email:", personalData.Email);

                            // Document Information Section
                            AddSection(column, "ДАННИ ЗА ДОКУМЕНТ");
                            
                            AddField(column, "Номер на документ:", personalData.DocumentNumber);
                            AddField(column, "Дата на издаване:", personalData.DocumentIssueDate.ToString(FormConstants.BulgarianDateFormat));
                            AddField(column, "Издаден от:", personalData.DocumentIssuedBy);

                            // Declaration Text
                            column.Item().PaddingTop(20).Text(text =>
                            {
                                text.Span("Декларирам, че съм запознат/а с разпоредбите на Закона за защита на личните данни (ЗЗЛД) и давам съгласието си личните ми данни да бъдат обработвани за целите на административното обслужване.")
                                    .FontSize(10);
                            });

                            // Signature Section
                            column.Item().PaddingTop(30).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Дата: {DateTime.Now.ToString(FormConstants.BulgarianDateFormat)}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().AlignRight().Text("_____________________");
                                    col.Item().AlignRight().Text("(подпис)").FontSize(9);
                                });
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span($"Генериран на: {DateTime.Now.ToString(FormConstants.BulgarianDateFormat)} {DateTime.Now:HH:mm}")
                                .FontSize(8)
                                .Italic();
                        });
                });
            }).GeneratePdf();

            return pdfBytes;
        }, cancellationToken);
    }

    private static void AddSection(ColumnDescriptor column, string title)
    {
        column.Item().PaddingTop(10).Text(title)
            .Bold()
            .FontSize(12)
            .Underline();
    }

    private static void AddField(ColumnDescriptor column, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(150).Text(label).Bold();
            row.RelativeItem().Text(value ?? string.Empty);
        });
    }
}
