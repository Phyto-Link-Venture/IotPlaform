namespace IoTPlatform.Services.Authorization;

/// <summary>
/// Computes a user's effective permissions — the union of permission codes across all
/// user groups they belong to. Backs the <c>HasPermission</c> authorization policy.
/// </summary>
public interface IPermissionService
{
    Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default);
}
