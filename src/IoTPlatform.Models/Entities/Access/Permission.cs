using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Access;

/// <summary>
/// A grantable permission within a module. Permissions are no longer a fixed enum of
/// CRUD actions — each one binds directly to a controller action method. A module maps
/// to a controller (e.g. "Users") and <see cref="ActionMethod"/> maps to a method on it
/// (e.g. "createUser", "createSuperAdmin"). <see cref="Code"/> is the "{Module}.{ActionMethod}"
/// string referenced by <c>[HasPermission(...)]</c> and checked by the policy provider.
/// </summary>
public class Permission : AuditableEntity
{
    public Guid ModuleId { get; set; }

    /// <summary>The "{Module}.{ActionMethod}" code, e.g. "Users.createUser". Unique.</summary>
    public string Code { get; set; } = null!;

    /// <summary>Human-friendly display name, e.g. "Create User".</summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The controller action method this permission guards, e.g. "createUser".
    /// Bound manually in code via <c>[HasPermission(...)]</c>.
    /// </summary>
    public string ActionMethod { get; set; } = null!;

    public Module Module { get; set; } = null!;
    public ICollection<UserGroupPermission> UserGroupPermissions { get; set; } = new List<UserGroupPermission>();
}
