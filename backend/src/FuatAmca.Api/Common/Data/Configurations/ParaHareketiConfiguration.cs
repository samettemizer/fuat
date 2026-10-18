using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data.Configurations;

internal sealed class ParaHareketiConfiguration : IEntityTypeConfiguration<ParaHareketi>
{
    public void Configure(EntityTypeBuilder<ParaHareketi> builder)
    {
        builder.ToTable("ParaHareketleri");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.GiderTipi).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(h => h.Yon).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(h => h.Tutar).HasPrecision(18, 2);
        builder.Property(h => h.Description).HasMaxLength(500);

        builder.HasIndex(h => new { h.ApartmanId, h.IslemTarihi });
        builder.HasIndex(h => new { h.MemberId, h.GiderTipi });
        builder.HasIndex(h => h.Muhlet);

        // Ayni donem / gider tipi icin bir uyeye iki kez tahakkuk yazilamaz.
        builder.HasIndex(h => new { h.MemberId, h.GiderTipi, h.DonemYil, h.DonemAy })
            .IsUnique()
            .HasDatabaseName("IX_ParaHareketleri_Tahakkuk_Unique")
            .HasFilter("\"Yon\" = 'Tahakkuk'");
    }
}
