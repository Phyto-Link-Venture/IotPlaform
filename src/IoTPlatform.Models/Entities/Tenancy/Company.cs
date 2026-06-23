using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Tenancy;

public class Company : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
