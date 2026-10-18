using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Members.GetMember;

/// <summary>
/// Uye detayini bakiye ozeti, kalem kirilimi ve son on para hareketi ile dondurur.
/// Bakiye durumu ve muhlet uyarisi SQL'e cevrilemedigi icin bellekte hesaplanir.
/// </summary>
public sealed class GetMemberHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    private const int RecentHareketCount = 10;

    public async Task<Result<GetMemberResponse>> HandleAsync(
        Guid apartmanId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<GetMemberResponse>(accessResult.Error!);
        }

        var raw = await dbContext.Members
            .AsNoTracking()
            .Where(u => u.Id == memberId && u.ApartmanId == apartmanId)
            .Select(u => new
            {
                u.Id,
                u.ApartmanId,
                u.FirstName,
                u.LastName,
                u.DaireNo,
                u.PhoneNumber,
                u.SakinTipi,
                u.EvSahibiFullName,
                u.EvSahibiPhoneNumber,
                u.Email,
                u.Notes,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt,
                TahakkukToplam = u.Bakiye != null ? u.Bakiye.TahakkukToplam : 0m,
                OdemeToplam = u.Bakiye != null ? u.Bakiye.OdemeToplam : 0m,
                Bakiye = u.Bakiye != null ? u.Bakiye.Bakiye : 0m,
                EnYakinMuhlet = u.Bakiye != null ? u.Bakiye.EnYakinMuhlet : null,
                Kalemler = u.BakiyeKalemleri
                    .OrderBy(k => k.GiderTipi)
                    .Select(k => new MemberBakiyeKalemiResponse(
                        k.GiderTipi,
                        k.TahakkukToplam,
                        k.OdemeToplam,
                        k.Bakiye))
                    .ToList(),
                RecentHareketler = u.ParaHareketleri
                    .OrderByDescending(h => h.IslemTarihi)
                    .ThenByDescending(h => h.RecordedAt)
                    .Take(RecentHareketCount)
                    .Select(h => new MemberRecentHareketResponse(
                        h.IslemTarihi,
                        h.GiderTipi,
                        h.Yon,
                        h.Tutar,
                        h.DonemYil,
                        h.DonemAy,
                        h.Muhlet,
                        h.Description))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (raw is null)
        {
            return Result.Failure<GetMemberResponse>(Error.NotFound(
                "member.not_found",
                "Üye kaydı bulunamadı."));
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        return Result.Success(new GetMemberResponse(
            raw.Id,
            raw.ApartmanId,
            raw.FirstName,
            raw.LastName,
            $"{raw.FirstName} {raw.LastName}",
            raw.DaireNo,
            raw.PhoneNumber,
            raw.SakinTipi,
            raw.EvSahibiFullName,
            raw.EvSahibiPhoneNumber,
            raw.Email,
            raw.Notes,
            raw.IsActive,
            raw.CreatedAt,
            raw.UpdatedAt,
            raw.TahakkukToplam,
            raw.OdemeToplam,
            raw.Bakiye,
            BakiyeHesaplayici.Durum(raw.Bakiye),
            raw.EnYakinMuhlet,
            BakiyeHesaplayici.MuhletGectiMi(raw.EnYakinMuhlet, raw.Bakiye, today),
            raw.Kalemler,
            raw.RecentHareketler));
    }
}
