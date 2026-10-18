using FuatAmca.Api.Common.Constants;
using Microsoft.AspNetCore.Identity;

namespace FuatAmca.Api.Common.Entities;

/// <summary>
/// Panel rolu. Rol adlari <see cref="FuatAmcaRoles"/> icinde sabit olarak tutulur.
/// </summary>
public sealed class Role : IdentityRole<Guid>
{
    public Role() { }

    public Role(string roleName) : base(roleName) { }

    /// <summary>Rolun panelde gosterilecek aciklamasi.</summary>
    public string Description { get; set; } = string.Empty;
}
