namespace FuatAmca.Api.Features.ParaHareketleri.GecikmisMuhletler;

/// <summary>
/// Muhleti gelmis / gecmis ve halen borclu olan uye satiri.
/// Panelde toast uyarisi bu listeden uretilir.
/// </summary>
public sealed record GecikmisMuhletResponse(
    Guid MemberId,
    string FullName,
    string DaireNo,
    string PhoneNumber,
    DateOnly Muhlet,
    int GecikmeGunu,
    decimal Bakiye);
