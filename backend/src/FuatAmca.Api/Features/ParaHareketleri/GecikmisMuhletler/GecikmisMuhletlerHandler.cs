using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.ParaHareketleri.GecikmisMuhletler;

/// <summary>
/// Muhlet tarihi gelmis / gecmis ve bakiyesi halen borclu olan uyeleri listeler.
/// Panel bu listeyi acilista toast uyarisi gostermek icin kullanir.
/// Read-only: AsNoTracking + tek sorguda DTO projeksiyonu.
/// </summary>
public sealed class GecikmisMuhletlerHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    public async Task<Result<IReadOnlyList<GecikmisMuhletResponse>>> HandleAsync(
        Guid apartmanId,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<GecikmisMuhletResponse>>(accessResult.Error!);
        }

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var items = await dbContext.Members
            .AsNoTracking()
            .Where(u => u.ApartmanId == apartmanId
                        && u.Bakiye != null
                        && u.Bakiye.Bakiye > 0m
                        && u.ParaHareketleri.Any(h => h.Muhlet != null && h.Muhlet <= today))
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.DaireNo,
                u.PhoneNumber,
                Bakiye = u.Bakiye!.Bakiye,
                // Uye bazinda en yakin (en eski) gecikmis muhlet.
                Muhlet = u.ParaHareketleri
                    .Where(h => h.Muhlet != null && h.Muhlet <= today)
                    .Min(h => h.Muhlet)
            })
            .OrderBy(row => row.Muhlet)
            .ToListAsync(cancellationToken);

        var results = items
            .Where(row => row.Muhlet is not null)
            .Select(row => new GecikmisMuhletResponse(
                row.Id,
                $"{row.FirstName} {row.LastName}",
                row.DaireNo,
                row.PhoneNumber,
                row.Muhlet!.Value,
                today.DayNumber - row.Muhlet.Value.DayNumber,
                row.Bakiye))
            .ToList();

        return Result.Success<IReadOnlyList<GecikmisMuhletResponse>>(results);
    }
}
