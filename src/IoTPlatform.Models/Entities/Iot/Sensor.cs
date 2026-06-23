using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Iot;

public class Sensor : AuditableEntity
{
    public Guid DeviceId { get; set; }
    public string SensorType { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;

    public Device Device { get; set; } = null!;
    public ICollection<TelemetryData> Telemetry { get; set; } = new List<TelemetryData>();
}
