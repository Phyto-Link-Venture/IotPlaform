using IoTPlatform.Models.Common;
using IoTPlatform.Models.Enums;

namespace IoTPlatform.Models.Entities.Iot;

public class Device : TenantEntity
{
    public Guid? DepartmentId { get; set; }
    public Guid DeviceTypeId { get; set; }

    /// <summary>Stable external identifier used on the MQTT topic / by the device firmware.</summary>
    public string DeviceKey { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;
    public DateTimeOffset? LastSeenAt { get; set; }

    public DeviceType DeviceType { get; set; } = null!;
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    public ICollection<DeviceCommand> Commands { get; set; } = new List<DeviceCommand>();
}
