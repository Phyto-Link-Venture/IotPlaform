using System.Security.Claims;
using IoTPlatform.Common.Abstractions;

namespace IoTPlatform.API.Authentication;

/// <summary>Resolves the current user from the JWT claims on the active HTTP request.</summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public bool IsSuperAdmin =>
        string.Equals(Principal?.FindFirstValue(AppClaimTypes.IsSuperAdmin), "true", StringComparison.OrdinalIgnoreCase);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
