using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Enums;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Apartmanlar.ApartmanOlustur;

/// <summary>
/// Yeni apartman kaydini, olusturan kullanicinin yetkisini ve dort temel gider
/// kaleminin varsayilan ayarlarini tek transaction icinde olusturur.
/// </summary>
public sealed class ApartmanOlusturHandler(
    FuatAmcaDbContext dbContext,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<ApartmanOlusturResponse>> HandleAsync(
        ApartmanOlusturRequest request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.GetRequiredUserId();
        var now = timeProvider.GetUtcNow();
        var name = request.Name.Trim();

        var hasDuplicateName = await dbContext.ApartmanUsers
            .AnyAsync(ak => ak.UserId == userId && ak.Apartman!.Name == name, cancellationToken);

        if (hasDuplicateName)
        {
            return Result.Failure<ApartmanOlusturResponse>(
                Error.Conflict("apartman.duplicate_name", "Bu adla kayıtlı bir apartmanınız zaten var."));
        }

        var apartman = new Apartman
        {
            Name = name,
            Address = request.Address?.Trim(),
            IsinmaTipi = request.IsinmaTipi,
            SonOdemeGunu = request.SonOdemeGunu,
            IsActive = true,
            CreatedAt = now,
            CreatedByUserId = userId
        };

        var tutarlar = request.GiderTutarlari ?? new GiderTutarlariRequest(0m, 0m, 0m, 0m);

        // Bireysel kombili binada yakit kalemi 0 TL ile baslar.
        var yakitTutari = request.IsinmaTipi == IsinmaTipi.BireyselKombi ? 0m : tutarlar.Yakit;

        apartman.GiderAyarlari =
        [
            GiderAyariOlustur(GiderTipi.Aidat, tutarlar.Aidat, kiracidanAlinirMi: true, now),
            GiderAyariOlustur(GiderTipi.Yakit, yakitTutari, kiracidanAlinirMi: true, now),
            // Bina tadilati kiracidan tahsil edilmez.
            GiderAyariOlustur(GiderTipi.BinaTadilati, tutarlar.BinaTadilati, kiracidanAlinirMi: false, now),
            GiderAyariOlustur(GiderTipi.SairGider, tutarlar.SairGider, kiracidanAlinirMi: true, now)
        ];

        apartman.Users =
        [
            new ApartmanUser
            {
                ApartmanId = apartman.Id,
                UserId = userId,
                AddedAt = now
            }
        ];

        dbContext.Apartmanlar.Add(apartman);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new ApartmanOlusturResponse(
            apartman.Id,
            apartman.Name,
            apartman.IsinmaTipi,
            apartman.SonOdemeGunu,
            apartman.CreatedAt));
    }

    private static ApartmanGiderAyari GiderAyariOlustur(
        GiderTipi giderTipi,
        decimal tutar,
        bool kiracidanAlinirMi,
        DateTimeOffset now) => new()
    {
        GiderTipi = giderTipi,
        VarsayilanTutar = decimal.Round(tutar, 2, MidpointRounding.AwayFromZero),
        KiracidanAlinirMi = kiracidanAlinirMi,
        IsActive = true,
        UpdatedAt = now
    };
}
