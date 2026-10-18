using FuatAmca.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Common.Data;

/// <summary>
/// Bakim komutlari. Migration ve seed islemleri acik komutla da uygulanabilir:
///   dotnet FuatAmca.Api.dll --migrate --seed
/// docker-compose bu komutu ayri bir tek-seferlik servis olarak calistirir.
/// </summary>
public static class DatabaseCommands
{
    public const string MigrateCommand = "--migrate";

    public const string SeedCommand = "--seed";

    /// <summary>Komut satirinda bakim komutu var mi.</summary>
    public static bool HasMaintenanceCommand(string[] args) =>
        args.Contains(MigrateCommand, StringComparer.Ordinal)
        || args.Contains(SeedCommand, StringComparer.Ordinal);

    /// <summary>Istenen bakim komutlarini sirayla calistirir; cikis kodunu dondurur.</summary>
    public static async Task<int> RunAsync(IServiceProvider serviceProvider, string[] args)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("FuatAmca.DatabaseCommands");

        try
        {
            if (args.Contains(MigrateCommand, StringComparer.Ordinal))
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FuatAmcaDbContext>();
                logger.LogInformation("Applying migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Migrations applied.");
            }

            if (args.Contains(SeedCommand, StringComparer.Ordinal))
            {
                await SeedAsync(scope.ServiceProvider, logger);
            }

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Maintenance command failed.");
            return 1;
        }
    }

    /// <summary>Rolleri ve (yapilandirildiysa) ilk yonetici hesabini olusturur.</summary>
    private static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var settings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SeedSettings>>().Value;

        var roleDescriptions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FuatAmcaRoles.Admin] = "Apartman yoneticisi, tam yetkili",
            [FuatAmcaRoles.AssistantAdmin] = "Uye ve duyuru yonetimi",
            [FuatAmcaRoles.Auditor] = "Sadece okuma yetkisi",
            [FuatAmcaRoles.Treasurer] = "Para hareketleri ve tahakkuk yonetimi"
        };

        foreach (var (roleName, description) in roleDescriptions)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role(roleName) { Description = description });
                logger.LogInformation("Role created: {Role}", roleName);
            }
        }

        if (string.IsNullOrWhiteSpace(settings.AdminEmail) || string.IsNullOrWhiteSpace(settings.AdminPassword))
        {
            logger.LogWarning(
                "Admin account was not created because Seed:AdminEmail or Seed:AdminPassword is missing.");
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(settings.AdminEmail);
        if (existingUser is not null)
        {
            logger.LogInformation("Admin account already exists: {Email}", settings.AdminEmail);
            return;
        }

        var admin = new User
        {
            UserName = settings.AdminEmail,
            Email = settings.AdminEmail,
            EmailConfirmed = true,
            FullName = settings.AdminFullName,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, settings.AdminPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "Admin account could not be created: " + string.Join(", ", result.Errors.Select(h => h.Description)));
        }

        await userManager.AddToRoleAsync(admin, FuatAmcaRoles.Admin);
        logger.LogInformation("Initial admin account created: {Email}", settings.AdminEmail);
    }
}
