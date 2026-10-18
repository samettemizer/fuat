using System.Security.Claims;

namespace FuatAmca.Api.Common.Security;

/// <summary>
/// Istegi yapan panel kullanicisinin kimligi.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);

    /// <summary>Kullanici kimligi yoksa hata firlatir (endpoint authorize edilmisse guvenlidir).</summary>
    Guid GetRequiredUserId();
}

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(t => t.Value).ToArray() ?? [];

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;

    public Guid GetRequiredUserId() =>
        UserId ?? throw new InvalidOperationException("The request does not contain a user identifier.");
}
