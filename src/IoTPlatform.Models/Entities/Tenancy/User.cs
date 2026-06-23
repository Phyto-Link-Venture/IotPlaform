using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Tenancy;

public class User : TenantEntity
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<UserGroupMember> GroupMemberships { get; set; } = new List<UserGroupMember>();
}
