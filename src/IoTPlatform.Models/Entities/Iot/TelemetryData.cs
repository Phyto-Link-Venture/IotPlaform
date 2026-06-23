namespace IoTPlatform.Models.Entities.Iot;

/// <summary>
/// A single time-series reading. Kept deliberately lean and append-only.
/// Indexed on (device_id, sensor_id, timestamp). Does not inherit the audit
/// base class — telemetry is immutable, high-volume, and never soft-deleted.
/// Designed to migrate to TimescaleDB / partitioning later without schema upheaval.
/// </summary>
public class TelemetryData
{
    public long Id { get; set; }
    public Guid DeviceId { get; set; }
    public Guid SensorId { get; set; }
    public double Value { get; set; }
    public string? RawPayload { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Free-form JSON metadata (jsonb).</summary>
    public string? Metadata { get; set; }

    public Device Device { get; set; } = null!;
    public Sensor Sensor { get; set; } = null!;
}
