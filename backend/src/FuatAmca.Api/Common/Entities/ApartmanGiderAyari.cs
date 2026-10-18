using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Apartmanin gider kalemi basina aylik varsayilan tutar ve tahsil kurallari.
/// </summary>
public sealed class ApartmanGiderAyari
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid ApartmanId { get; set; }

    public Apartman? Apartman { get; set; }

    public GiderTipi GiderTipi { get; set; }

    /// <summary>Her uye icin aylik tahakkuk edecek varsayilan tutar.</summary>
    public decimal VarsayilanTutar { get; set; }

    /// <summary>Kiraci uyelerden tahsil edilir mi. Bina tadilati icin false.</summary>
    public bool KiracidanAlinirMi { get; set; } = true;

    /// <summary>Pasif kalem aylik tahakkuk uretiminde atlanir.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
