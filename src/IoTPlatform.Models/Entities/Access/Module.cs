using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Access;

/// <summary>
/// A functional area of the platform (Dashboard, Settings, Reports, Devices...).
/// Modules are global (not tenant-scoped); permissions hang off them.
/// </summary>
public class Module : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
