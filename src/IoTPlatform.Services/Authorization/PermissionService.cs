using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Services.Authorization;

public sealed class PermissionService(AppDbContext db) : IPermissionService
{
    public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        // user -> groups -> group permissions -> permission codes (union, deduplicated).
        var codes = await db.UserGroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .SelectMany(m => m.UserGroup.Permissions)
            .Select(gp => gp.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var permissions = await GetEffectivePermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permissionCode);
    }
}
