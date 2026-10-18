namespace FuatAmca.Api.Common.Constants;

/// <summary>
/// Authorization politika adlari. Endpointler bu adlari kullanir.
/// </summary>
public static class FuatAmcaPolicies
{
    /// <summary>Panel kullanicisi ve rol yonetimi. Sadece Yonetici.</summary>
    public const string UserManagement = "UserManagement";

    /// <summary>Apartman olusturma / ayar guncelleme. Sadece Yonetici.</summary>
    public const string ApartmanManagement = "ApartmanManagement";

    /// <summary>Uye kayitlari ve duyurular. Yonetici + yardimcisi.</summary>
    public const string MemberManagement = "MemberManagement";

    /// <summary>Para hareketi ve tahakkuk islemleri. Yonetici + Sayman.</summary>
    public const string ParaHareketiManagement = "ParaHareketiManagement";

    /// <summary>Okuma yetkisi. Tum panel rolleri.</summary>
    public const string Read = "Read";
}
