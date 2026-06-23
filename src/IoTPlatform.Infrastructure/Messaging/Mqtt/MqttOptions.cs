namespace IoTPlatform.Infrastructure.Messaging.Mqtt;

/// <summary>Bound from configuration section "Mqtt".</summary>
public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string ClientId { get; set; } = "iotplatform-api";

    /// <summary>Topic filter the platform subscribes to for inbound telemetry.</summary>
    public string TelemetryTopic { get; set; } = "devices/+/telemetry";

    /// <summary>Topic template used when publishing commands back to devices ({deviceKey} is substituted).</summary>
    public string CommandTopicTemplate { get; set; } = "devices/{deviceKey}/commands";

    public bool Enabled { get; set; } = true;
}
