namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Panelden yonetilen apartman duyurusu.
/// </summary>
public sealed class Duyuru
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid ApartmanId { get; set; }

    public Apartman? Apartman { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateOnly PublishedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>Duyurunun gecerlilik bitisi; bos ise sinirsiz.</summary>
    public DateOnly? ExpiresOn { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
