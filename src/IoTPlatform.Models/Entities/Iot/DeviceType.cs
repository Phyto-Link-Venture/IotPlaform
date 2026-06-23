using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Iot;

public class DeviceType : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
