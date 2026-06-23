namespace IoTPlatform.Common.Abstractions;

/// <summary>
/// Provides information about the currently authenticated user.
/// Implemented in the API layer (resolved from the JWT / HttpContext) and
/// consumed by the persistence interceptors and services for auditing.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsSuperAdmin { get; }
    bool IsAuthenticated { get; }
}
