namespace FuatAmca.Api.Common.Constants;

/// <summary>
/// Panele giris yapabilen kullanici rolleri.
/// </summary>
public static class FuatAmcaRoles
{
    /// <summary>Apartman yoneticisi; tam yetkili.</summary>
    public const string Admin = "Admin";

    /// <summary>Yonetici yardimcisi; uye ve duyuru yonetimi yapar.</summary>
    public const string AssistantAdmin = "AssistantAdmin";

    /// <summary>Denetci; sadece okuma yetkisi.</summary>
    public const string Auditor = "Auditor";

    /// <summary>Sayman; para hareketleri ve tahakkuk yonetimi.</summary>
    public const string Treasurer = "Treasurer";

    /// <summary>Tum roller.</summary>
    public static readonly string[] All = [Admin, AssistantAdmin, Auditor, Treasurer];

    /// <summary>Verilen rol adi tanimli mi.</summary>
    public static bool IsValid(string? role) =>
        role is not null && All.Contains(role, StringComparer.Ordinal);
}
