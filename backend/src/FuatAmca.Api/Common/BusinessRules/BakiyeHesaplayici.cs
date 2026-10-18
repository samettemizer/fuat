using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Common.BusinessRules;

/// <summary>
/// Bakiye hesap kurallari. Saf fonksiyonlardir, unit test hedefidir.
/// </summary>
public static class BakiyeHesaplayici
{
    /// <summary>Bakiye = tahakkuk - odeme. Pozitif borc, negatif alacak demektir.</summary>
    public static decimal Bakiye(decimal tahakkukToplam, decimal odemeToplam) =>
        decimal.Round(tahakkukToplam - odemeToplam, 2, MidpointRounding.AwayFromZero);

    /// <summary>Hareketin tahakkuk / odeme toplamlarina etkisi.</summary>
    public static (decimal TahakkukEtkisi, decimal OdemeEtkisi) Etki(HareketYonu yon, decimal tutar) =>
        yon switch
        {
            HareketYonu.Tahakkuk => (tutar, 0m),
            HareketYonu.Odeme => (0m, tutar),
            _ => (0m, 0m)
        };

    /// <summary>Datagrid renklendirmesi icin bakiye durumu.</summary>
    public static BakiyeDurumu Durum(decimal bakiye) => bakiye switch
    {
        > 0m => BakiyeDurumu.Borclu,
        < 0m => BakiyeDurumu.Alacakli,
        _ => BakiyeDurumu.Dengede
    };

    /// <summary>
    /// Muhlet uyarisi gosterilmeli mi: muhlet tarihi gelmis / gecmis ve uye halen borclu.
    /// </summary>
    public static bool MuhletGectiMi(DateOnly? muhlet, decimal bakiye, DateOnly today) =>
        muhlet is not null && bakiye > 0m && muhlet.Value <= today;
}
