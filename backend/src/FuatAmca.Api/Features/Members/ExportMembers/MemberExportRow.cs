using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Features.Members.ExportMembers;

/// <summary>
/// Excel / pdf ureteclerinin okudugu hazir satir. Metinler son kullaniciya
/// gorunecegi icin Turkce yazilir, durum alani satir renklendirmesini belirler.
/// </summary>
internal sealed record MemberExportRow(
    string DaireNo,
    string FullName,
    string PhoneNumber,
    string SakinTipiLabel,
    decimal TahakkukToplam,
    decimal OdemeToplam,
    decimal Bakiye,
    BakiyeDurumu Status);
