namespace FuatAmca.Api.Common.Enums;

/// <summary>
/// Dairelerin aylik temel gider kalemleri.
/// </summary>
public enum GiderTipi
{
    /// <summary>Aylik aidat.</summary>
    Aidat = 1,

    /// <summary>Yakit gideri. Bina bireysel kombi ile isiniyorsa tahakkuk 0 olur.</summary>
    Yakit = 2,

    /// <summary>Bina tadilati. Kiraci uyelerden tahsil edilmez.</summary>
    BinaTadilati = 3,

    /// <summary>Yukaridaki kalemlere girmeyen diger giderler.</summary>
    SairGider = 4
}
