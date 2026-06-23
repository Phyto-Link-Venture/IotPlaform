using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Tenancy;

/// <summary>
/// Join entity: a user can belong to many user groups, and a group has many users.
/// Effective permissions are the union of permissions across a user's groups.
/// </summary>
public class UserGroupMember : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid UserGroupId { get; set; }

    public User User { get; set; } = null!;
    public UserGroup UserGroup { get; set; } = null!;
}
