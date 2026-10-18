using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data.Configurations;

internal sealed class ApartmanGiderAyariConfiguration : IEntityTypeConfiguration<ApartmanGiderAyari>
{
    public void Configure(EntityTypeBuilder<ApartmanGiderAyari> builder)
    {
        builder.ToTable("ApartmanGiderAyarlari");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.GiderTipi).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(g => g.VarsayilanTutar).HasPrecision(18, 2);

        // Bir apartmanda her gider tipi icin tek ayar kaydi bulunur.
        builder.HasIndex(g => new { g.ApartmanId, g.GiderTipi }).IsUnique();
    }
}
