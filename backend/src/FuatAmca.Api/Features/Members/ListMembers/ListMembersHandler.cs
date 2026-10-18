using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Pagination;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Members.ListMembers;

/// <summary>
/// Apartmanin uyelerini filtreli / sirali / sayfali dondurur ve filtre kapsamindaki
/// borc - alacak toplamlarini hesaplar. Bakiye durumu ile muhlet uyarisi SQL'e
/// cevrilemedigi icin ham alanlar cekilip bellekte hesaplanir.
/// </summary>
public sealed class ListMembersHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    public async Task<Result<ListMembersResponse>> HandleAsync(
        Guid apartmanId,
        ListMembersRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<ListMembersResponse>(accessResult.Error!);
        }

        var (page, pageSize) = PaginationRules.Normalize(request.Page, request.PageSize);
        var query = ApplyFilters(dbContext.Members.AsNoTracking(), apartmanId, request);

        // Toplamlar ve kayit sayisi tek sorguda: apartman icinde tek grup olusur.
        // Bakiye once duz kolona indirilir, boylece gruplama saf SQL'e cevrilir.
        var summary = await query
            .Select(u => new
            {
                u.ApartmanId,
                Bakiye = u.Bakiye != null ? u.Bakiye.Bakiye : 0m
            })
            .GroupBy(row => row.ApartmanId)
            .Select(group => new
            {
                TotalCount = group.Count(),
                ToplamBorc = group.Sum(row => row.Bakiye > 0m ? row.Bakiye : 0m),
                ToplamAlacak = group.Sum(row => row.Bakiye < 0m ? -row.Bakiye : 0m),
                BorcluSayisi = group.Sum(row => row.Bakiye > 0m ? 1 : 0),
                AlacakliSayisi = group.Sum(row => row.Bakiye < 0m ? 1 : 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (summary is null)
        {
            return Result.Success(new ListMembersResponse(
                PagedResult<MemberRowResponse>.Empty(page, pageSize),
                0m,
                0m,
                0,
                0));
        }

        var rawRecords = await ApplySorting(query, request.SortBy, request.SortDirection)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.DaireNo,
                u.PhoneNumber,
                u.SakinTipi,
                u.EvSahibiFullName,
                u.IsActive,
                TahakkukToplam = u.Bakiye != null ? u.Bakiye.TahakkukToplam : 0m,
                OdemeToplam = u.Bakiye != null ? u.Bakiye.OdemeToplam : 0m,
                Bakiye = u.Bakiye != null ? u.Bakiye.Bakiye : 0m,
                EnYakinMuhlet = u.Bakiye != null ? u.Bakiye.EnYakinMuhlet : null
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var rows = rawRecords.ConvertAll(raw => new MemberRowResponse(
            raw.Id,
            raw.FirstName,
            raw.LastName,
            $"{raw.FirstName} {raw.LastName}",
            raw.DaireNo,
            raw.PhoneNumber,
            raw.SakinTipi,
            raw.EvSahibiFullName,
            raw.IsActive,
            raw.TahakkukToplam,
            raw.OdemeToplam,
            raw.Bakiye,
            BakiyeHesaplayici.Durum(raw.Bakiye),
            raw.EnYakinMuhlet,
            BakiyeHesaplayici.MuhletGectiMi(raw.EnYakinMuhlet, raw.Bakiye, today)));

        return Result.Success(new ListMembersResponse(
            new PagedResult<MemberRowResponse>(rows, summary.TotalCount, page, pageSize),
            summary.ToplamBorc,
            summary.ToplamAlacak,
            summary.BorcluSayisi,
            summary.AlacakliSayisi));
    }

    /// <summary>Arama, sakin tipi, bakiye durumu ve aktiflik filtreleri.</summary>
    private static IQueryable<Member> ApplyFilters(IQueryable<Member> query, Guid apartmanId, ListMembersRequest request)
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

        query = request.BakiyeDurumu switch
        {
            BakiyeDurumu.Borclu => query.Where(u => u.Bakiye != null && u.Bakiye.Bakiye > 0m),
            BakiyeDurumu.Alacakli => query.Where(u => u.Bakiye != null && u.Bakiye.Bakiye < 0m),
            BakiyeDurumu.Dengede => query.Where(u => u.Bakiye == null || u.Bakiye.Bakiye == 0m),
            _ => query
        };

        return query;
    }

    /// <summary>Desteklenen siralama alanlari; bilinmeyen deger daire no siralamasina duser.</summary>
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

        // Sayfalamanin kararli olmasi icin son kirilim kimlik.
        return ordered.ThenBy(u => u.Id);
    }
}
