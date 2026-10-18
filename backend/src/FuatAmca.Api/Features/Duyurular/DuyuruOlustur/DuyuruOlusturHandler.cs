using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Duyurular.DuyuruOlustur;

/// <summary>
/// Apartmana yeni duyuru kaydeder. Yayin tarihi verilmezse bugunden yayina girer.
/// </summary>
public sealed class DuyuruOlusturHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<DuyuruOlusturResponse>> HandleAsync(
        Guid apartmanId,
        DuyuruOlusturRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<DuyuruOlusturResponse>(accessResult.Error!);
        }

        var now = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var duyuru = new Duyuru
        {
            ApartmanId = apartmanId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            PublishedOn = request.PublishedOn ?? today,
            ExpiresOn = request.ExpiresOn,
            IsActive = true,
            CreatedByUserId = currentUser.GetRequiredUserId(),
            CreatedAt = now
        };

        dbContext.Duyurular.Add(duyuru);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new DuyuruOlusturResponse(
            duyuru.Id,
            duyuru.ApartmanId,
            duyuru.Title,
            duyuru.Content,
            duyuru.PublishedOn,
            duyuru.ExpiresOn,
            duyuru.IsActive,
            duyuru.CreatedAt));
    }
}
