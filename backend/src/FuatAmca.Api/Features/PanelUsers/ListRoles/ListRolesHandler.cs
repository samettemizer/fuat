using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FuatAmca.Api.Common.Results;
using FuatAmca.Api.Common.Entities;

namespace FuatAmca.Api.Features.PanelUsers.ListRoles;

/// <summary>
/// Kullanici tanimlama ekranlarinda kullanilan rol listesi.
/// Read-only: AsNoTracking + dogrudan DTO projeksiyonu.
/// </summary>
public sealed class ListRolesHandler(RoleManager<Role> roleManager)
{
    public async Task<Result<IReadOnlyList<RoleResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        var roles = await roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new RoleResponse(role.Name!, role.Description))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<RoleResponse>>(roles);
    }
}
