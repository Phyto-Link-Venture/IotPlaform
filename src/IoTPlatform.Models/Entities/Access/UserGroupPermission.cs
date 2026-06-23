using IoTPlatform.Models.Common;
using IoTPlatform.Models.Entities.Tenancy;

namespace IoTPlatform.Models.Entities.Access;

/// <summary>
/// Join entity granting a <see cref="Permission"/> to a <see cref="UserGroup"/>.
/// Permissions are assigned at the group level, never directly to users.
/// </summary>
public class UserGroupPermission : AuditableEntity
{
    public Guid UserGroupId { get; set; }
    public Guid PermissionId { get; set; }

    public UserGroup UserGroup { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
