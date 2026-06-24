using System.Text.Json;
using IoTPlatform.Common.Constants;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Models.Entities.Iot;
using IoTPlatform.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IoTPlatform.Infrastructure.Messaging.Mqtt;

/// <summary>
/// Default ingestor: deserializes a <see cref="TelemetryEnvelope"/>, resolves the device,
/// validates the dataname_symbol, then appends a telemetry row directly at the device level
/// and refreshes the device's last-seen timestamp. Runs with query filters ignored because
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

        if (!TelemetryConventions.IsValidDataNameSymbol(envelope.DataNameSymbol))
        {
            logger.LogWarning(
                "Telemetry for device {DeviceKey} has invalid dataNameSymbol '{DataNameSymbol}'; discarded",
                envelope.DeviceKey, envelope.DataNameSymbol);
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

        db.TelemetryData.Add(new TelemetryData
        {
            DeviceId = device.Id,
            DataNameSymbol = envelope.DataNameSymbol,
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
