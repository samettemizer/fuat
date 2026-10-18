namespace FuatAmca.Api.Common.Data;

/// <summary>
/// Ilk kurulumda olusturulacak yonetici hesabi. Parola env / user-secrets ile verilir.
/// </summary>
public sealed class SeedSettings
{
    public const string SectionName = "Seed";

    public string? AdminEmail { get; set; }

    public string? AdminPassword { get; set; }

    public string AdminFullName { get; set; } = "Apartman Yoneticisi";
}
