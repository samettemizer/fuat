using FuatAmca.Api.Common.Enums;

namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Uyenin gider kalemi bazinda bakiye kirilimi (aidat, yakit, bina tadilati, sair gider).
/// </summary>
public sealed class MemberBakiyeKalemi
{
    public Guid MemberId { get; set; }

    public Member? Member { get; set; }

    public Guid ApartmanId { get; set; }

    public GiderTipi GiderTipi { get; set; }

    public decimal TahakkukToplam { get; set; }

    public decimal OdemeToplam { get; set; }

    /// <summary>TahakkukToplam - OdemeToplam.</summary>
    public decimal Bakiye { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
