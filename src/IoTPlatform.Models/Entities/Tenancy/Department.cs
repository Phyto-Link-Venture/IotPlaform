using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Tenancy;

public class Department : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
