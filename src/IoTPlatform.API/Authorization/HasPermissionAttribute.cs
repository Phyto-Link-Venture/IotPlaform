using Microsoft.AspNetCore.Authorization;

namespace IoTPlatform.API.Authorization;

/// <summary>
/// Requires the current user to hold a specific permission code, e.g.
/// <c>[HasPermission(Permissions.Device.Control)]</c>. Backed by a dynamic policy.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "perm:";

    public HasPermissionAttribute(string permission) => Policy = PolicyPrefix + permission;

    public static string? PermissionFromPolicy(string policyName) =>
        policyName.StartsWith(PolicyPrefix, StringComparison.Ordinal)
            ? policyName[PolicyPrefix.Length..]
            : null;
}
