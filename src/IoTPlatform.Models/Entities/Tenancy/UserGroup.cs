using IoTPlatform.Models.Common;
using IoTPlatform.Models.Entities.Access;

namespace IoTPlatform.Models.Entities.Tenancy;

public class UserGroup : TenantEntity
{
    public Guid? DepartmentId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Department? Department { get; set; }
    public ICollection<UserGroupMember> Members { get; set; } = new List<UserGroupMember>();
    public ICollection<UserGroupPermission> Permissions { get; set; } = new List<UserGroupPermission>();
}
