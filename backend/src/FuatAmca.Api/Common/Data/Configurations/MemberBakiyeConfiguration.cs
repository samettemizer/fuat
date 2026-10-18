using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data.Configurations;

internal sealed class MemberBakiyeConfiguration : IEntityTypeConfiguration<MemberBakiye>
{
    public void Configure(EntityTypeBuilder<MemberBakiye> builder)
    {
        builder.ToTable("MemberBakiyeleri");
        builder.HasKey(b => b.MemberId);

        builder.Property(b => b.TahakkukToplam).HasPrecision(18, 2);
        builder.Property(b => b.OdemeToplam).HasPrecision(18, 2);
        builder.Property(b => b.Bakiye).HasPrecision(18, 2);

        builder.HasIndex(b => new { b.ApartmanId, b.Bakiye });
    }
}

internal sealed class MemberBakiyeKalemiConfiguration : IEntityTypeConfiguration<MemberBakiyeKalemi>
{
    public void Configure(EntityTypeBuilder<MemberBakiyeKalemi> builder)
    {
        builder.ToTable("MemberBakiyeKalemleri");
        builder.HasKey(k => new { k.MemberId, k.GiderTipi });

        builder.Property(k => k.GiderTipi).HasConversion<string>().HasMaxLength(32);
        builder.Property(k => k.TahakkukToplam).HasPrecision(18, 2);
        builder.Property(k => k.OdemeToplam).HasPrecision(18, 2);
        builder.Property(k => k.Bakiye).HasPrecision(18, 2);

        builder.HasIndex(k => k.ApartmanId);
    }
}
