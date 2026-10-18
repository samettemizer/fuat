namespace FuatAmca.Api.Common.Enums;

/// <summary>
/// Uyenin dairede hangi statude oturdugu.
/// </summary>
public enum SakinTipi
{
    /// <summary>Daire sahibi.</summary>
    EvSahibi = 1,

    /// <summary>Kiraci. Ev sahibi bilgisi zorunludur, bina tadilati tahakkuk etmez.</summary>
    Kiraci = 2
}
