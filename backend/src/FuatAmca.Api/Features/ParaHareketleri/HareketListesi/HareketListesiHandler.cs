using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Pagination;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.ParaHareketleri.HareketListesi;

/// <summary>
/// Apartmanin hareket defterini filtreli ve sayfali dondurur.
/// Read-only: AsNoTracking + dogrudan DTO projeksiyonu; toplamlar ve sayfa
/// icerigi iki sorguda alinir, satir basina ek sorgu yapilmaz.
/// </summary>
public sealed class HareketListesiHandler(FuatAmcaDbContext dbContext, ApartmanAccessService accessService)
{
    public async Task<Result<HareketListesiResponse>> HandleAsync(
        Guid apartmanId,
        HareketListesiRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<HareketListesiResponse>(accessResult.Error!);
        }

        var (page, pageSize) = PaginationRules.Normalize(request.Page, request.PageSize);

        var query = dbContext.ParaHareketleri
            .AsNoTracking()
            .Where(h => h.ApartmanId == apartmanId);

        if (request.MemberId is not null)
        {
            query = query.Where(h => h.MemberId == request.MemberId.Value);
        }

        if (request.GiderTipi is not null)
        {
            query = query.Where(h => h.GiderTipi == request.GiderTipi.Value);
        }

        if (request.Yon is not null)
        {
            query = query.Where(h => h.Yon == request.Yon.Value);
        }

        if (request.StartDate is not null)
        {
            query = query.Where(h => h.IslemTarihi >= request.StartDate.Value);
        }

        if (request.EndDate is not null)
        {
            query = query.Where(h => h.IslemTarihi <= request.EndDate.Value);
        }

        if (request.DonemYil is not null)
        {
            query = query.Where(h => h.DonemYil == request.DonemYil.Value);
        }

        if (request.DonemAy is not null)
        {
            query = query.Where(h => h.DonemAy == request.DonemAy.Value);
        }

        // Kayit sayisi ve yon bazli toplamlar tek aggregate sorgusunda alinir.
        // Sorgu tek apartmana filtreli oldugu icin ApartmanId sabit bir gruplama anahtaridir.
        var summary = await query
            .GroupBy(h => h.ApartmanId)
            .Select(group => new
            {
                Count = group.Count(),
                Tahakkuk = group.Sum(h => h.Yon == HareketYonu.Tahakkuk ? h.Tutar : 0m),
                Odeme = group.Sum(h => h.Yon == HareketYonu.Odeme ? h.Tutar : 0m)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (summary is null)
        {
            return Result.Success(new HareketListesiResponse(
                PagedResult<HareketRowResponse>.Empty(page, pageSize),
                0m,
                0m));
        }

        var items = await query
            .OrderByDescending(h => h.IslemTarihi)
            .ThenByDescending(h => h.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new HareketRowResponse(
                h.Id,
                h.MemberId,
                h.Member!.FirstName + " " + h.Member.LastName,
                h.Member.DaireNo,
                h.GiderTipi,
                h.Yon,
                h.Tutar,
                h.IslemTarihi,
                h.DonemYil,
                h.DonemAy,
                h.Muhlet,
                h.Description,
                h.RecordedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new HareketListesiResponse(
            new PagedResult<HareketRowResponse>(items, summary.Count, page, pageSize),
            summary.Tahakkuk,
            summary.Odeme));
    }
}
