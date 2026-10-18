using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Members.ExportMembers;

/// <summary>
/// Uye listesini xlsx veya pdf dosyasi olarak uretir. Filtreler uye listesi ile
/// aynidir ancak sayfalama uygulanmaz; en fazla <see cref="MaxRecords"/> satir alinir.
/// </summary>
public sealed class ExportMembersHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    /// <summary>Tek dosyaya alinabilecek en fazla uye sayisi.</summary>
    private const int MaxRecords = 5000;

    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string PdfContentType = "application/pdf";

    public async Task<Result<ExportMembersResponse>> HandleAsync(
        Guid apartmanId,
        ExportMembersRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<ExportMembersResponse>(accessResult.Error!);
        }

        var format = string.IsNullOrWhiteSpace(request.Format) ? "xlsx" : request.Format.Trim().ToLowerInvariant();
        if (format is not ("xlsx" or "pdf"))
        {
            return Result.Failure<ExportMembersResponse>(Error.Validation(
                "export.invalid_format",
                "Dışa aktarım formatı yalnızca xlsx veya pdf olabilir."));
        }

        var apartmanName = await dbContext.Apartmanlar
            .AsNoTracking()
            .Where(a => a.Id == apartmanId)
            .Select(a => a.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (apartmanName is null)
        {
            return Result.Failure<ExportMembersResponse>(Error.NotFound(
                "apartman.not_found",
                "Apartman kaydı bulunamadı."));
        }

        var query = ApplyFilters(dbContext.Members.AsNoTracking(), apartmanId, request);

        var rawRecords = await ApplySorting(query, request.SortBy, request.SortDirection)
            .Take(MaxRecords)
            .Select(u => new
            {
                u.DaireNo,
                u.FirstName,
                u.LastName,
                u.PhoneNumber,
                u.SakinTipi,
                TahakkukToplam = u.Bakiye != null ? u.Bakiye.TahakkukToplam : 0m,
                OdemeToplam = u.Bakiye != null ? u.Bakiye.OdemeToplam : 0m,
                Bakiye = u.Bakiye != null ? u.Bakiye.Bakiye : 0m
            })
            .ToListAsync(cancellationToken);

        var rows = rawRecords.ConvertAll(raw => new MemberExportRow(
            raw.DaireNo,
            $"{raw.FirstName} {raw.LastName}",
            raw.PhoneNumber,
            raw.SakinTipi == SakinTipi.Kiraci ? "Kiracı" : "Ev Sahibi",
            raw.TahakkukToplam,
            raw.OdemeToplam,
            raw.Bakiye,
            BakiyeHesaplayici.Durum(raw.Bakiye)));

        var now = timeProvider.GetUtcNow();
        var fileNameStem = $"member-list-{Slug(apartmanName)}-{now.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture)}";

        return format == "pdf"
            ? Result.Success(new ExportMembersResponse(
                PdfGenerator.Generate(apartmanName, now, rows),
                $"{fileNameStem}.pdf",
                PdfContentType))
            : Result.Success(new ExportMembersResponse(
                ExcelGenerator.Generate(apartmanName, rows),
                $"{fileNameStem}.xlsx",
                XlsxContentType));
    }

    /// <summary>Uye listesi ile ayni filtre mantigi; dilimler bagimsiz oldugu icin tekrar yazilir.</summary>
    private static IQueryable<Member> ApplyFilters(
        IQueryable<Member> query,
        Guid apartmanId,
        ExportMembersRequest request)
    {
        query = query.Where(u => u.ApartmanId == apartmanId);

        var search = request.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.FirstName, pattern) ||
                EF.Functions.ILike(u.LastName, pattern) ||
                EF.Functions.ILike(u.FirstName + " " + u.LastName, pattern) ||
                EF.Functions.ILike(u.DaireNo, pattern) ||
                EF.Functions.ILike(u.PhoneNumber, pattern));
        }

        if (request.SakinTipi is not null)
        {
            var sakinTipi = request.SakinTipi.Value;
            query = query.Where(u => u.SakinTipi == sakinTipi);
        }

        if (request.ActiveOnly == true)
        {
            query = query.Where(u => u.IsActive);
        }

        return request.BakiyeDurumu switch
        {
            BakiyeDurumu.Borclu => query.Where(u => u.Bakiye != null && u.Bakiye.Bakiye > 0m),
            BakiyeDurumu.Alacakli => query.Where(u => u.Bakiye != null && u.Bakiye.Bakiye < 0m),
            BakiyeDurumu.Dengede => query.Where(u => u.Bakiye == null || u.Bakiye.Bakiye == 0m),
            _ => query
        };
    }

    /// <summary>Uye listesi ile ayni siralama alanlari.</summary>
    private static IQueryable<Member> ApplySorting(IQueryable<Member> query, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<Member> ordered = sortBy?.Trim().ToLowerInvariant() switch
        {
            "fullname" => descending
                ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            "bakiye" => descending
                ? query.OrderByDescending(u => u.Bakiye != null ? u.Bakiye.Bakiye : 0m)
                : query.OrderBy(u => u.Bakiye != null ? u.Bakiye.Bakiye : 0m),
            "createdat" => descending
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => descending
                ? query.OrderByDescending(u => u.DaireNo).ThenByDescending(u => u.FirstName)
                : query.OrderBy(u => u.DaireNo).ThenBy(u => u.FirstName)
        };

        return ordered.ThenBy(u => u.Id);
    }

    /// <summary>
    /// Dosya adi icin apartman adini ascii slug'a cevirir: Turkce harfler katlanir,
    /// alfanumerik olmayan her sey tek tireye iner.
    /// </summary>
    private static string Slug(string apartmanName)
    {
        // Noktasiz i ve noktali I unicode ayristirmasi ile katlanmadigi icin elle eslenir.
        var prepared = apartmanName
            .Replace('\u0131', 'i')
            .Replace('\u0130', 'i')
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(prepared.Length);
        foreach (var character in prepared)
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        return slug.Length == 0 ? "apartman" : slug;
    }
}
