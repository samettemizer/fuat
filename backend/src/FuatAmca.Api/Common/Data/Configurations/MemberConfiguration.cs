using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data.Configurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(80).IsRequired();
        builder.Property(u => u.DaireNo).HasMaxLength(10).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(10).IsRequired();
        builder.Property(u => u.SakinTipi).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(u => u.EvSahibiFullName).HasMaxLength(160);
        builder.Property(u => u.EvSahibiPhoneNumber).HasMaxLength(10);
        builder.Property(u => u.Email).HasMaxLength(256);
        builder.Property(u => u.PasswordHash).HasMaxLength(256);
        builder.Property(u => u.Notes).HasMaxLength(1000);

        // Ayni apartmanda bir cep telefonu bir kez kayitli olabilir.
        builder.HasIndex(u => new { u.ApartmanId, u.PhoneNumber }).IsUnique();
        builder.HasIndex(u => new { u.ApartmanId, u.DaireNo });
        builder.HasIndex(u => u.PhoneNumber);

        builder.HasOne(u => u.Bakiye!)
            .WithOne(b => b.Member!)
            .HasForeignKey<MemberBakiye>(b => b.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.BakiyeKalemleri)
            .WithOne(k => k.Member!)
            .HasForeignKey(k => k.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ParaHareketleri)
            .WithOne(h => h.Member!)
            .HasForeignKey(h => h.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
