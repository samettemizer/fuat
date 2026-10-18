using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Common.BusinessRules;

/// <summary>
/// Aylik gider tahakkuku kurallari:
/// - Bina bireysel kombi ile isiniyorsa yakit tahakkuku 0 TL'dir.
/// - Bina tadilati kiraci uyeden tahsil edilmez (tahakkuk uretilmez).
/// - Kalem ayarinda KiracidanAlinirMi false ise kiraciya tahakkuk uretilmez.
/// </summary>
public static class TahakkukKurallari
{
    /// <summary>
    /// Uye icin tahakkuk edecek tutari belirler.
    /// null donerse o kalem icin kayit uretilmez.
    /// </summary>
    public static decimal? TutarBelirle(
        GiderTipi giderTipi,
        IsinmaTipi isinmaTipi,
        SakinTipi sakinTipi,
        decimal varsayilanTutar,
        bool kiracidanAlinirMi)
    {
        // Bina tadilati kiracidan alinmaz.
        if (giderTipi == GiderTipi.BinaTadilati && sakinTipi == SakinTipi.Kiraci)
        {
            return null;
        }

        // Kalem bazinda kiraci muafiyeti.
        if (sakinTipi == SakinTipi.Kiraci && !kiracidanAlinirMi)
        {
            return null;
        }

        // Bireysel kombili binada yakit bedeli olusmaz; kayit 0 TL olarak dusulur.
        if (giderTipi == GiderTipi.Yakit && isinmaTipi == IsinmaTipi.BireyselKombi)
        {
            return 0m;
        }

        return decimal.Round(varsayilanTutar, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Donem bilgisi gecerli mi (yil 2000-2100, ay 1-12).</summary>
    public static bool DonemGecerliMi(int yil, int ay) => yil is >= 2000 and <= 2100 && ay is >= 1 and <= 12;

    /// <summary>Donem etiketi: 2026-03 gibi.</summary>
    public static string DonemEtiketi(int? yil, int? ay) =>
        yil is null || ay is null ? "-" : $"{yil.Value:0000}-{ay.Value:00}";
}
