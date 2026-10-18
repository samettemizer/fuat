using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Validation;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Members.UpdateMember;

/// <summary>
/// Uye kaydini guncelleyen komut. Telefon degistiginde apartman icindeki
/// tekillik kendi kaydi haric tutularak yeniden kontrol edilir.
/// </summary>
public sealed class UpdateMemberHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    public async Task<Result<UpdateMemberResponse>> HandleAsync(
        Guid apartmanId,
        Guid memberId,
        UpdateMemberRequest request,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<UpdateMemberResponse>(accessResult.Error!);
        }

        var phoneNumber = PhoneNumberRules.Normalize(request.PhoneNumber);
        if (phoneNumber is null)
        {
            return Result.Failure<UpdateMemberResponse>(Error.Validation(
                "member.invalid_phone",
                "Cep telefonu başında sıfır olmadan 10 haneli olmalıdır."));
        }

        var member = await dbContext.Members
            .FirstOrDefaultAsync(u => u.Id == memberId && u.ApartmanId == apartmanId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<UpdateMemberResponse>(Error.NotFound(
                "member.not_found",
                "Üye kaydı bulunamadı."));
        }

        if (!string.Equals(member.PhoneNumber, phoneNumber, StringComparison.Ordinal))
        {
            var isPhoneRegistered = await dbContext.Members
                .AnyAsync(
                    u => u.ApartmanId == apartmanId && u.Id != memberId && u.PhoneNumber == phoneNumber,
                    cancellationToken);

            if (isPhoneRegistered)
            {
                return Result.Failure<UpdateMemberResponse>(Error.Conflict(
                    "member.phone_registered",
                    "Bu cep telefonu bu apartmanda başka bir üyeye kayıtlı."));
            }
        }

        member.FirstName = request.FirstName.Trim();
        member.LastName = request.LastName.Trim();
        member.DaireNo = request.DaireNo.Trim();
        member.PhoneNumber = phoneNumber;
        member.SakinTipi = request.SakinTipi;
        member.EvSahibiFullName = request.EvSahibiFullName?.Trim();
        member.EvSahibiPhoneNumber = PhoneNumberRules.Normalize(request.EvSahibiPhoneNumber);
        member.Email = request.Email?.Trim();
        member.Notes = request.Notes?.Trim();
        member.IsActive = request.IsActive;
        member.UpdatedAt = timeProvider.GetUtcNow();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateMemberResponse(
            member.Id,
            member.FirstName,
            member.LastName,
            member.DaireNo,
            member.PhoneNumber,
            member.SakinTipi,
            member.EvSahibiFullName,
            member.EvSahibiPhoneNumber,
            member.Email,
            member.IsActive,
            member.UpdatedAt));
    }
}
