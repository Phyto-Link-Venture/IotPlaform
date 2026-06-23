namespace IoTPlatform.Models.Entities.Logging;

/// <summary>
/// Records who changed what. Append-only; not soft-deletable. Captures AI/MCP
/// invocations (model, tool calls) alongside ordinary entity changes.
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? UserId { get; set; }
    public string EntityType { get; set; } = null!;
    public string? EntityId { get; set; }
    public string Action { get; set; } = null!; // Create, Update, Delete, AiInvoke, McpCall...
    public string? OldValue { get; set; }       // jsonb
    public string? NewValue { get; set; }       // jsonb
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
