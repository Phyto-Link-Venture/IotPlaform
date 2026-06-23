using System.ComponentModel;
using IoTPlatform.Services.Ai.Tools;
using ModelContextProtocol.Server;

namespace IoTPlatform.API.Mcp;

/// <summary>
/// MCP tools exposed to external LLMs. They delegate to the same <see cref="DeviceAiTools"/>
/// used by the in-platform assistant, keeping a single source of truth. Access is scoped and
/// audited; tool parameters are injected from request services by the MCP SDK.
/// </summary>
[McpServerToolType]
public class PlatformMcpTools
{
    [McpServerTool(Name = "get_device_status")]
    [Description("Gets the current status and last-seen time of a device by its device key.")]
    public static Task<string> GetDeviceStatus(
        DeviceAiTools tools,
        [Description("The device key / identifier")] string deviceKey,
        CancellationToken cancellationToken)
        => tools.GetDeviceStatusAsync(deviceKey, cancellationToken);

    [McpServerTool(Name = "query_recent_telemetry")]
    [Description("Returns the most recent telemetry readings for a device, newest first.")]
    public static Task<string> QueryRecentTelemetry(
        DeviceAiTools tools,
        [Description("The device key / identifier")] string deviceKey,
        [Description("Maximum number of readings to return (1-100)")] int limit,
        CancellationToken cancellationToken)
        => tools.QueryRecentTelemetryAsync(deviceKey, limit, cancellationToken);
}
