using System.ComponentModel;
using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Services.Ai.Tools;

/// <summary>
/// Tools ("skills") exposed to the LLM via function calling. The same methods back the
/// MCP server, so there is a single source of truth. Queries respect the tenant query
/// filter because they run within the user's request scope.
/// </summary>
public sealed class DeviceAiTools(AppDbContext db)
{
    [Description("Gets the current status and last-seen time of a device by its device key.")]
    public async Task<string> GetDeviceStatusAsync(
        [Description("The device key / identifier")] string deviceKey,
        CancellationToken cancellationToken = default)
    {
        var device = await db.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);

        return device is null
            ? $"No device found with key '{deviceKey}'."
            : $"Device '{device.Name}' ({device.DeviceKey}) is {device.Status}. Last seen: {device.LastSeenAt?.ToString("u") ?? "never"}.";
    }

    [Description("Returns the most recent telemetry readings for a device, newest first.")]
    public async Task<string> QueryRecentTelemetryAsync(
        [Description("The device key / identifier")] string deviceKey,
        [Description("Maximum number of readings to return (1-100)")] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);

        var device = await db.Devices.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);
        if (device is null)
            return $"No device found with key '{deviceKey}'.";

        var readings = await db.TelemetryData.AsNoTracking()
            .Where(t => t.DeviceId == device.Id)
            .OrderByDescending(t => t.Timestamp)
            .Take(limit)
            .Select(t => new { t.SensorId, t.Value, t.Timestamp })
            .ToListAsync(cancellationToken);

        if (readings.Count == 0)
            return $"No telemetry recorded for device '{deviceKey}'.";

        return string.Join("\n", readings.Select(r => $"{r.Timestamp:u}: sensor {r.SensorId} = {r.Value}"));
    }
}
