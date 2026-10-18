namespace FuatAmca.Api.Common.Enums;

/// <summary>
/// Binanin isinma sekli. Yakit tahakkukunu belirler.
/// </summary>
public enum IsinmaTipi
{
    /// <summary>Merkezi sistem; yakit gideri uyelere paylastirilir.</summary>
    MerkeziSistem = 1,

    /// <summary>Bireysel kombi; yakit tahakkuku her uye icin 0 TL olur.</summary>
    BireyselKombi = 2
}
