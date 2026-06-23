namespace IoTPlatform.Infrastructure.Messaging.Mqtt;

/// <summary>Persists an inbound MQTT telemetry payload. Implementations run outside any
/// tenant/user request context, so they bypass the multi-tenant query filter explicitly.</summary>
public interface ITelemetryIngestor
{
    Task IngestAsync(string topic, string payload, CancellationToken cancellationToken = default);
}
