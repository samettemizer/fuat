using System.Globalization;
using FuatAmca.Api.Common.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FuatAmca.Api.Features.Members.ExportMembers;

/// <summary>
/// Uye listesini A4 yatay pdf tablosu olarak uretir. Borclu satirlar acik kirmizi,
/// alacakli satirlar acik yesil zemin alir. Yazi tipi QuestPDF ile gomulu gelen Lato.
/// </summary>
internal static class PdfGenerator
{
    private const string HeaderBackground = "#EEEEEE";

    private const string BorcluBackground = "#FFEBEB";

    private const string AlacakliBackground = "#E8F5E9";

    private const string BorderColor = "#BDBDBD";

    private const string SecondaryTextColor = "#616161";

    /// <summary>Tutar bicimi: binlik nokta, ondalik virgul (kultur ayarindan bagimsiz).</summary>
    private static readonly NumberFormatInfo AmountFormat = new()
    {
        NumberDecimalSeparator = ",",
        NumberGroupSeparator = ".",
        NumberDecimalDigits = 2
    };

    internal static byte[] Generate(string apartmanName, DateTimeOffset now, IReadOnlyList<MemberExportRow> rows)
    {
        var document = Document.Create(builder =>
        {
            builder.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(9));

                page.Header().PaddingBottom(8).Column(column =>
                {
                    column.Item().Text($"{apartmanName} - Üye Listesi").FontSize(14).SemiBold();
                    column.Item().Text(
                            $"Rapor tarihi: {now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)} - Kayıt sayısı: {rows.Count}")
                        .FontSize(8)
                        .FontColor(SecondaryTextColor);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(55);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(85);
                        columns.ConstantColumn(70);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Daire No");
                        header.Cell().Element(HeaderCell).Text("Ad Soyad");
                        header.Cell().Element(HeaderCell).Text("Cep Telefonu");
                        header.Cell().Element(HeaderCell).Text("Sakin Tipi");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Tahakkuk");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Ödeme");
                        header.Cell().Element(HeaderCell).AlignRight().Text("Bakiye");
                    });

                    foreach (var row in rows)
                    {
                        var background = GetBackgroundColor(row.Status);

                        table.Cell().Element(cell => DataCell(cell, background)).Text(row.DaireNo);
                        table.Cell().Element(cell => DataCell(cell, background)).Text(row.FullName);
                        table.Cell().Element(cell => DataCell(cell, background)).Text(row.PhoneNumber);
                        table.Cell().Element(cell => DataCell(cell, background)).Text(row.SakinTipiLabel);
                        table.Cell().Element(cell => DataCell(cell, background)).AlignRight()
                            .Text(FormatAmount(row.TahakkukToplam));
                        table.Cell().Element(cell => DataCell(cell, background)).AlignRight()
                            .Text(FormatAmount(row.OdemeToplam));
                        table.Cell().Element(cell => DataCell(cell, background)).AlignRight()
                            .Text(FormatAmount(row.Bakiye)).SemiBold();
                    }
                });

                page.Footer()
                    .AlignRight()
                    .DefaultTextStyle(style => style.FontSize(8).FontColor(SecondaryTextColor))
                    .Text(text =>
                    {
                        text.Span("Sayfa ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatAmount(decimal tutar) => tutar.ToString("N2", AmountFormat);

    private static string? GetBackgroundColor(BakiyeDurumu status) => status switch
    {
        BakiyeDurumu.Borclu => BorcluBackground,
        BakiyeDurumu.Alacakli => AlacakliBackground,
        _ => null
    };

    private static IContainer HeaderCell(IContainer cell) => cell
        .Background(HeaderBackground)
        .BorderBottom(1)
        .BorderColor(BorderColor)
        .PaddingVertical(4)
        .PaddingHorizontal(3)
        .DefaultTextStyle(style => style.SemiBold());

    private static IContainer DataCell(IContainer cell, string? background)
    {
        var prepared = cell
            .BorderBottom(1)
            .BorderColor(BorderColor)
            .PaddingVertical(3)
            .PaddingHorizontal(3);

        return background is null ? prepared : prepared.Background(background);
    }
}
