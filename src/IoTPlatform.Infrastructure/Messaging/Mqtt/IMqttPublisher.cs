namespace IoTPlatform.Infrastructure.Messaging.Mqtt;

/// <summary>Publishes messages (e.g. device commands) to the MQTT broker.</summary>
public interface IMqttPublisher
{
    Task PublishCommandAsync(string deviceKey, string payload, CancellationToken cancellationToken = default);
    Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default);
}
