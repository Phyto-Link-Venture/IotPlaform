using IoTPlatform.Common.Abstractions;
using IoTPlatform.Services.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace IoTPlatform.API.Authorization;

/// <summary>
/// Grants access when the user's effective permissions (union across their user groups)
/// include the required code. SuperAdmin bypasses all permission checks.
/// </summary>
public sealed class PermissionAuthorizationHandler(
    ICurrentUser currentUser,
    IPermissionService permissionService) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (currentUser.IsSuperAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (currentUser.UserId is { } userId &&
            await permissionService.HasPermissionAsync(userId, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
