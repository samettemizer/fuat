using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore; // MigrateAsync uzantısı için gerekli olabilir
using FuatAmca.Api.Common.DependencyInjection;
using FuatAmca.Api.Common.Data;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

// Acilis loglari icin bootstrap logger; asil yapilandirma appsettings'ten gelir.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configure) => configure
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("application", "fuat-api")
        .WriteTo.Console());

    builder.Services.Configure<SeedSettings>(
        builder.Configuration.GetSection(SeedSettings.SectionName));

    builder.AddFuatAmcaServices();

    // QuestPDF community lisansi (uye listesi pdf disa aktarimi icin).
    QuestPDF.Settings.License = LicenseType.Community;

    var app = builder.Build();

    // MIGRATION:
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            Log.Information("Applying database migrations...");
            var context = services.GetRequiredService<FuatAmcaDbContext>();

            // Bekleyen tüm migration'ları uygular, veritabanı yoksa oluşturur.
            await context.Database.MigrateAsync();
            Log.Information("Database migrations applied.");
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Database migration failed during startup.");
        }
    }

    // Bakim modu: migration / seed uygulanip cikilir, HTTP sunucusu acilmaz.
    if (DatabaseCommands.HasMaintenanceCommand(args))
    {
        return await DatabaseCommands.RunAsync(app.Services, args);
    }

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseSerilogRequestLogging();
    app.UseCors(ServiceRegistration.CorsPolicyName);
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapOpenApi();
    app.MapScalarApiReference("/scalar");

    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
        .AllowAnonymous();
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready")
    }).AllowAnonymous();

    app.MapFuatAmcaEndpoints();

    Log.Information("Starting FuatAmca API. Environment: {Environment}", app.Environment.EnvironmentName);
    await app.RunAsync();

    return 0;
}
// HostAbortedException: "dotnet ef" tasarim zamani host'u bilerek durdurur, hata degildir.
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "FuatAmca API failed to start.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Entegrasyon testlerindeki public WebApplicationFactory<Program> icin Program sinifi disari acilir.
public partial class Program { }
