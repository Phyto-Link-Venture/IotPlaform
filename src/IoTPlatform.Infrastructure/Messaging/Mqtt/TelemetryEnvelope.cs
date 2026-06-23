namespace IoTPlatform.Infrastructure.Messaging.Mqtt;

/// <summary>
/// Expected JSON shape of an inbound telemetry message. Adjust to match device firmware.
/// Example: {"deviceKey":"dev-001","sensorType":"temperature","value":21.5,"timestamp":"2026-01-01T00:00:00Z"}
/// </summary>
public sealed class TelemetryEnvelope
{
    public string DeviceKey { get; set; } = null!;
    public string SensorType { get; set; } = null!;
    public double Value { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public string? Metadata { get; set; }
}
