namespace IoTPlatform.Models.Entities.Iot;

/// <summary>
/// A single time-series reading. Kept deliberately lean and append-only.
/// Telemetry posts directly at the device level — the sensor layer is hidden — and
/// each reading is identified by <see cref="DataNameSymbol"/> (e.g. "temperature_C").
/// Indexed on (device_id, dataname_symbol, timestamp). Does not inherit the audit
/// base class — telemetry is immutable, high-volume, and never soft-deleted.
/// Designed to migrate to TimescaleDB / partitioning later without schema upheaval.
/// </summary>
public class TelemetryData
{
    public long Id { get; set; }
    public Guid DeviceId { get; set; }

    /// <summary>
    /// Identifies the measurement stream in "dataname_symbol" form, e.g.
    /// "temperature_C", "humidity_percent". Validated on ingestion.
    /// </summary>
    public string DataNameSymbol { get; set; } = null!;

    public double Value { get; set; }
    public string? RawPayload { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>Free-form JSON metadata (jsonb).</summary>
    public string? Metadata { get; set; }

    public Device Device { get; set; } = null!;
}
