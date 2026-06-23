using IoTPlatform.Models.Common;
using IoTPlatform.Models.Enums;

namespace IoTPlatform.Models.Entities.Access;

/// <summary>
/// A grantable permission within a module, e.g. "Device.Control".
/// </summary>
public class Permission : AuditableEntity
{
    public Guid ModuleId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public PermissionAction Action { get; set; }

    public Module Module { get; set; } = null!;
    public ICollection<UserGroupPermission> UserGroupPermissions { get; set; } = new List<UserGroupPermission>();
}
