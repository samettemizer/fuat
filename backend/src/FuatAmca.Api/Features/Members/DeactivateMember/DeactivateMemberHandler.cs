using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Security;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Data;

namespace FuatAmca.Api.Features.Members.DeactivateMember;

/// <summary>
/// Uyeyi pasife alir (soft delete). Kayit silinmez; para hareketleri ve bakiye
/// gecmisi denetim icin oldugu gibi korunur.
/// </summary>
public sealed class DeactivateMemberHandler(
    FuatAmcaDbContext dbContext,
    ApartmanAccessService accessService,
    TimeProvider timeProvider)
{
    public async Task<Result<DeactivateMemberResponse>> HandleAsync(
        Guid apartmanId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var accessResult = await accessService.CheckAccessAsync(apartmanId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure<DeactivateMemberResponse>(accessResult.Error!);
        }

        var member = await dbContext.Members
            .FirstOrDefaultAsync(u => u.Id == memberId && u.ApartmanId == apartmanId, cancellationToken);

        if (member is null)
        {
            return Result.Failure<DeactivateMemberResponse>(Error.NotFound(
                "member.not_found",
                "Üye kaydı bulunamadı."));
        }

        if (member.IsActive)
        {
            member.IsActive = false;
            member.UpdatedAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new DeactivateMemberResponse(member.Id, member.IsActive));
    }
}
