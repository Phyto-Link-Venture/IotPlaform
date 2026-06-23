using MQTTnet;
using MQTTnet.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IoTPlatform.Infrastructure.Messaging.Mqtt;

/// <summary>
/// Long-running MQTT client: connects to the broker, subscribes to the telemetry topic,
/// and dispatches each inbound message to a scoped <see cref="ITelemetryIngestor"/>.
/// Also serves as the <see cref="IMqttPublisher"/> for outbound commands.
/// Registered as a singleton hosted service.
/// </summary>
public sealed class MqttBackgroundService : BackgroundService, IMqttPublisher
{
    private readonly MqttOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttBackgroundService> _logger;
    private readonly IMqttClient _client;
    private readonly MqttClientOptions _clientOptions;

    public MqttBackgroundService(
        IOptions<MqttOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<MqttBackgroundService> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;

        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();

        var builder = new MqttClientOptionsBuilder()
            .WithClientId(_options.ClientId)
            .WithTcpServer(_options.Host, _options.Port);

        if (!string.IsNullOrEmpty(_options.Username))
            builder = builder.WithCredentials(_options.Username, _options.Password);

        _clientOptions = builder.Build();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("MQTT ingestion disabled by configuration");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(_clientOptions, stoppingToken);
                    await _client.SubscribeAsync(
                        new MqttTopicFilterBuilder()
                            .WithTopic(_options.TelemetryTopic)
                            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                            .Build(),
                        stoppingToken);
                    _logger.LogInformation(
                        "Connected to MQTT broker {Host}:{Port}, subscribed to {Topic}",
                        _options.Host, _options.Port, _options.TelemetryTopic);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT connection error; retrying in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = e.ApplicationMessage.ConvertPayloadToString() ?? string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var ingestor = scope.ServiceProvider.GetRequiredService<ITelemetryIngestor>();
            await ingestor.IngestAsync(topic, payload, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ingest MQTT message on {Topic}", topic);
        }
    }

    public Task PublishCommandAsync(string deviceKey, string payload, CancellationToken cancellationToken = default)
    {
        var topic = _options.CommandTopicTemplate.Replace("{deviceKey}", deviceKey);
        return PublishAsync(topic, payload, cancellationToken);
    }

    public async Task PublishAsync(string topic, string payload, CancellationToken cancellationToken = default)
    {
        if (!_client.IsConnected)
            await _client.ConnectAsync(_clientOptions, cancellationToken);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _client.PublishAsync(message, cancellationToken);
    }

    public override void Dispose()
    {
        _client.Dispose();
        base.Dispose();
    }
}
