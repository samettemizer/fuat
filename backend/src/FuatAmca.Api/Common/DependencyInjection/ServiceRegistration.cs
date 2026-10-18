using FuatAmca.Api.Common.Constants;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Exceptions;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Common.DependencyInjection;

/// <summary>
/// Uygulama servis kayitlari. Program.cs'i kisa tutmak icin buraya toplandi.
/// </summary>
public static class ServiceRegistration
{
    public const string CorsPolicyName = "FuatAmcaPanel";

    public static WebApplicationBuilder AddFuatAmcaServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // PostgreSQL migrations are applied after Build() in Program or through --migrate.
        services.AddDbContext<FuatAmcaDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FuatAmcaDatabase"),
                npgsql => npgsql.MigrationsHistoryTable("__FuatAmcaMigrations")));

        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ApartmanAccessService>();
        services.AddScoped<BakiyeGuncelleyici>();
        services.AddSingleton<SigningKeyProvider>();
        services.AddScoped<TokenGenerator>();

        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<FuatAmcaDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // JWT dogrulama parametreleri imza anahtari servisinden beslenir.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<SigningKeyProvider, IOptions<JwtSettings>>((options, signingKeyProvider, jwtSettings) =>
            {
                var jwt = jwtSettings.Value;
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKeyProvider.Key,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(FuatAmcaPolicies.UserManagement, policy => policy.RequireRole(FuatAmcaRoles.Admin))
            .AddPolicy(FuatAmcaPolicies.ApartmanManagement, policy => policy.RequireRole(FuatAmcaRoles.Admin))
            .AddPolicy(FuatAmcaPolicies.MemberManagement, policy => policy.RequireRole(
                FuatAmcaRoles.Admin, FuatAmcaRoles.AssistantAdmin))
            .AddPolicy(FuatAmcaPolicies.ParaHareketiManagement, policy => policy.RequireRole(
                FuatAmcaRoles.Admin, FuatAmcaRoles.Treasurer))
            .AddPolicy(FuatAmcaPolicies.Read, policy => policy.RequireRole(FuatAmcaRoles.All));

        // FluentValidation: validatorler DI'dan cozulur, endpoint filtresinde calisir.
        services.AddValidatorsFromAssembly(typeof(ServiceRegistration).Assembly, includeInternalTypes: true);
        services.RegisterHandlers(typeof(ServiceRegistration).Assembly);

        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            });

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddOpenApi(options => options.AddDocumentTransformer<OpenApiSecurityTransformer>());

        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("FuatAmcaDatabase") ?? string.Empty,
                name: "postgresql",
                tags: ["ready"]);

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins);
            }

            policy.AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Content-Disposition");
        }));

        return builder;
    }
}
