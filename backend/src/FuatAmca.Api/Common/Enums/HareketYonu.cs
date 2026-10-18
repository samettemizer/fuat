namespace FuatAmca.Api.Common.Enums;

/// <summary>
/// Para hareketinin uye bakiyesini hangi yonde etkiledigi.
/// </summary>
public enum HareketYonu
{
    /// <summary>Uyeye borc yazilmasi (aylik gider tahakkuku).</summary>
    Tahakkuk = 1,

    /// <summary>Uyenin yaptigi odeme.</summary>
    Odeme = 2
}
