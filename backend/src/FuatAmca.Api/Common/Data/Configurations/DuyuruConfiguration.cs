using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data.Configurations;

internal sealed class DuyuruConfiguration : IEntityTypeConfiguration<Duyuru>
{
    public void Configure(EntityTypeBuilder<Duyuru> builder)
    {
        builder.ToTable("Duyurular");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Content).HasMaxLength(4000).IsRequired();

        builder.HasIndex(d => new { d.ApartmanId, d.IsActive, d.PublishedOn });
    }
}
