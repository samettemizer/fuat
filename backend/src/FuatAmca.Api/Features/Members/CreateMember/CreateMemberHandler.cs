using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.BusinessRules;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Members.CreateMember;

/// <summary>
/// Apartmana yeni uye kaydi ekler. Cep telefonu apartman icinde tekildir
/// ve uye ile birlikte bos master bakiye kaydi olusturulur.
/// </summary>
public sealed class CreateMemberHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    BakiyeGuncelleyici bakiyeGuncelleyici,
    TimeProvider timeProvider)
{
    public async Task<Result<CreateMemberResponse>> HandleAsync(
        Guid apartmanId,
        CreateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<CreateMemberResponse>(accessResult.Error!);
        }

        var phoneNumber = PhoneNumberRules.Normalize(request.PhoneNumber);
        if (phoneNumber is null)
        {
            return Result.Failure<CreateMemberResponse>(Error.Validation(
                "member.invalid_phone",
                "Cep telefonu başında sıfır olmadan 10 haneli olmalıdır."));
        }

        var isPhoneRegistered = await dbContext.Members
            .AnyAsync(u => u.ApartmanId == apartmanId && u.PhoneNumber == phoneNumber, cancellationToken);

        if (isPhoneRegistered)
        {
            return Result.Failure<CreateMemberResponse>(Error.Conflict(
                "member.phone_registered",
                "Bu cep telefonu bu apartmanda başka bir üyeye kayıtlı."));
        }

        var now = timeProvider.GetUtcNow();

        var member = new Member
        {
            ApartmanId = apartmanId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DaireNo = request.DaireNo.Trim(),
            PhoneNumber = phoneNumber,
            SakinTipi = request.SakinTipi,
            EvSahibiFullName = request.EvSahibiFullName?.Trim(),
            EvSahibiPhoneNumber = PhoneNumberRules.Normalize(request.EvSahibiPhoneNumber),
            Email = request.Email?.Trim(),
            Notes = request.Notes?.Trim(),
            IsActive = true,
            CreatedAt = now
        };

        // Master bakiye kaydi uye ile ayni transaction icinde sifir bakiye olarak acilir.
        member.Bakiye = bakiyeGuncelleyici.BaslangicBakiyesiOlustur(member.Id, apartmanId);

        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateMemberResponse(
            member.Id,
            member.FirstName,
            member.LastName,
            member.DaireNo,
            member.PhoneNumber,
            member.SakinTipi,
            member.IsActive,
            member.CreatedAt));
    }
}
