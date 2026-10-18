using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data.Configurations;

internal sealed class ApartmanUserConfiguration : IEntityTypeConfiguration<ApartmanUser>
{
    public void Configure(EntityTypeBuilder<ApartmanUser> builder)
    {
        builder.ToTable("ApartmanUsers");
        builder.HasKey(ak => new { ak.ApartmanId, ak.UserId });

        builder.HasOne(ak => ak.Apartman!)
            .WithMany(a => a.Users)
            .HasForeignKey(ak => ak.ApartmanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ak => ak.User!)
            .WithMany(k => k.ApartmanGrants)
            .HasForeignKey(ak => ak.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ak => ak.UserId);
    }
}
