using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data;

/// <summary>
/// FuatAmca veritabani baglami. EF Core'un kendisi Unit of Work, DbSet'ler de
/// repository oldugu icin ayrica bir repository / IUnitOfWork katmani yoktur;
/// feature handler'lari bu baglami dogrudan kullanir.
/// </summary>
public sealed class FuatAmcaDbContext(DbContextOptions<FuatAmcaDbContext> options)
    : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Apartman> Apartmanlar => Set<Apartman>();

    public DbSet<ApartmanGiderAyari> ApartmanGiderAyarlari => Set<ApartmanGiderAyari>();

    public DbSet<ApartmanUser> ApartmanUsers => Set<ApartmanUser>();

    public DbSet<Member> Members => Set<Member>();

    public DbSet<MemberBakiye> MemberBakiyeleri => Set<MemberBakiye>();

    public DbSet<MemberBakiyeKalemi> MemberBakiyeKalemleri => Set<MemberBakiyeKalemi>();

    public DbSet<ParaHareketi> ParaHareketleri => Set<ParaHareketi>();

    public DbSet<Duyuru> Duyurular => Set<Duyuru>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Identity tables use the shared technical naming convention.
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("Users");
            builder.Property(k => k.FullName).HasMaxLength(160).IsRequired();
        });

        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("Roles");
            builder.Property(r => r.Description).HasMaxLength(200);
        });

        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>()
            .ToTable("UserRoles");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>()
            .ToTable("UserClaims");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>()
            .ToTable("UserLogins");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>()
            .ToTable("UserTokens");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>()
            .ToTable("RoleClaims");

        // Feature disi tum varlik yapilandirmalari bu assembly'den yuklenir.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuatAmcaDbContext).Assembly);
    }
}
