using ClosedXML.Excel;
using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.ExportMembers;

/// <summary>
/// Uye listesini xlsx olarak uretir. Borclu satirlar acik kirmizi,
/// alacakli satirlar acik yesil zemin ile isaretlenir.
/// </summary>
internal static class ExcelGenerator
{
    private const int ColumnCount = 7;

    private const string AmountFormat = "#,##0.00";

    private static readonly string[] Headers =
        ["Daire No", "Ad Soyad", "Cep Telefonu", "Sakin Tipi", "Tahakkuk", "Ödeme", "Bakiye"];

    internal static byte[] Generate(string apartmanName, IReadOnlyList<MemberExportRow> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(GetWorksheetName(apartmanName));

        for (var column = 0; column < ColumnCount; column++)
        {
            worksheet.Cell(1, column + 1).Value = Headers[column];
        }

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(238, 238, 238);
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var rowNumber = 2;
        foreach (var row in rows)
        {
            worksheet.Cell(rowNumber, 1).Value = row.DaireNo;
            worksheet.Cell(rowNumber, 2).Value = row.FullName;
            worksheet.Cell(rowNumber, 3).Value = row.PhoneNumber;
            worksheet.Cell(rowNumber, 4).Value = row.SakinTipiLabel;
            worksheet.Cell(rowNumber, 5).Value = (double)row.TahakkukToplam;
            worksheet.Cell(rowNumber, 6).Value = (double)row.OdemeToplam;
            worksheet.Cell(rowNumber, 7).Value = (double)row.Bakiye;

            var amountRange = worksheet.Range(rowNumber, 5, rowNumber, ColumnCount);
            amountRange.Style.NumberFormat.Format = AmountFormat;

            var background = GetBackgroundColor(row.Status);
            if (background is not null)
            {
                worksheet.Range(rowNumber, 1, rowNumber, ColumnCount).Style.Fill.BackgroundColor = background;
            }

            rowNumber++;
        }

        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns(1, ColumnCount).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>Borclu acik kirmizi, alacakli acik yesil; dengede satir standart kalir.</summary>
    private static XLColor? GetBackgroundColor(BakiyeDurumu status) => status switch
    {
        BakiyeDurumu.Borclu => XLColor.FromArgb(255, 235, 235),
        BakiyeDurumu.Alacakli => XLColor.FromArgb(232, 245, 233),
        _ => null
    };

    /// <summary>Excel sayfa adi kisitlari: 31 karakter ve bazi isaretler yasak.</summary>
    private static string GetWorksheetName(string apartmanName)
    {
        var sanitized = string.Concat(apartmanName.Where(character => !"[]:*?/\\".Contains(character))).Trim();
        if (sanitized.Length == 0)
        {
            return "Uye Listesi";
        }

        return sanitized.Length > 31 ? sanitized[..31] : sanitized;
    }
}
