using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data.Configurations;

internal sealed class ApartmanConfiguration : IEntityTypeConfiguration<Apartman>
{
    public void Configure(EntityTypeBuilder<Apartman> builder)
    {
        builder.ToTable("Apartmanlar");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(160).IsRequired();
        builder.Property(a => a.Address).HasMaxLength(400);
        builder.Property(a => a.IsinmaTipi).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.HasIndex(a => a.CreatedByUserId);

        builder.HasMany(a => a.GiderAyarlari)
            .WithOne(g => g.Apartman!)
            .HasForeignKey(g => g.ApartmanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Members)
            .WithOne(u => u.Apartman!)
            .HasForeignKey(u => u.ApartmanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Duyurular)
            .WithOne(d => d.Apartman!)
            .HasForeignKey(d => d.ApartmanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
