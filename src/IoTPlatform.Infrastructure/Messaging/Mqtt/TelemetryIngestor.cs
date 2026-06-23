using System.Text.Json;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Models.Entities.Iot;
using IoTPlatform.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IoTPlatform.Infrastructure.Messaging.Mqtt;

/// <summary>
/// Default ingestor: deserializes a <see cref="TelemetryEnvelope"/>, resolves the device
/// (and auto-provisions the sensor on first sight), then appends a telemetry row and
/// refreshes the device's last-seen timestamp. Runs with query filters ignored because
/// MQTT ingestion has no ambient tenant/user context.
/// </summary>
public sealed class TelemetryIngestor(AppDbContext db, ILogger<TelemetryIngestor> logger) : ITelemetryIngestor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task IngestAsync(string topic, string payload, CancellationToken cancellationToken = default)
    {
        TelemetryEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<TelemetryEnvelope>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Discarding malformed telemetry on topic {Topic}", topic);
            return;
        }

        if (envelope is null || string.IsNullOrWhiteSpace(envelope.DeviceKey))
        {
            logger.LogWarning("Telemetry on topic {Topic} missing deviceKey; discarded", topic);
            return;
        }

        var device = await db.Devices
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.DeviceKey == envelope.DeviceKey && !d.IsDeleted, cancellationToken);

        if (device is null)
        {
            logger.LogWarning("Telemetry for unknown device {DeviceKey}; discarded", envelope.DeviceKey);
            return;
        }

        var sensor = await db.Sensors
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                s => s.DeviceId == device.Id && s.SensorType == envelope.SensorType && !s.IsDeleted,
                cancellationToken);

        if (sensor is null)
        {
            // Auto-provision a sensor the first time we see its type for this device.
            sensor = new Sensor
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                SensorType = envelope.SensorType,
                Name = envelope.SensorType,
            };
            db.Sensors.Add(sensor);
        }

        db.TelemetryData.Add(new TelemetryData
        {
            DeviceId = device.Id,
            SensorId = sensor.Id,
            Value = envelope.Value,
            RawPayload = payload,
            Metadata = envelope.Metadata,
            Timestamp = envelope.Timestamp ?? DateTimeOffset.UtcNow,
        });

        device.LastSeenAt = DateTimeOffset.UtcNow;
        device.Status = DeviceStatus.Online;

        await db.SaveChangesAsync(cancellationToken);
    }
}
