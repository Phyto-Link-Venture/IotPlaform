using IoTPlatform.Models.Common;
using IoTPlatform.Models.Enums;

namespace IoTPlatform.Models.Entities.Iot;

/// <summary>
/// A command issued back to a device (e.g. via MQTT publish). Tracks delivery state.
/// </summary>
public class DeviceCommand : AuditableEntity
{
    public Guid DeviceId { get; set; }
    public string Command { get; set; } = null!;
    public string? Payload { get; set; }
    public CommandStatus Status { get; set; } = CommandStatus.Pending;
    public Guid? IssuedBy { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }

    public Device Device { get; set; } = null!;
}
