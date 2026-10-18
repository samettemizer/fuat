namespace FuatAmca.Api.Common.Enums;

/// <summary>
/// Uye listesinde satir renklendirmesi icin bakiye durumu.
/// Borclu = acik kirmizi, Alacakli = acik yesil, Dengede = standart satir.
/// </summary>
public enum BakiyeDurumu
{
    Dengede = 0,
    Borclu = 1,
    Alacakli = 2
}
