using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Ai;

/// <summary>
/// An external LLM authorized to call this platform's MCP server. Access is scoped
/// and every call is audited. Only a hash of the API key is stored.
/// </summary>
public class McpClient : TenantEntity
{
    public string Name { get; set; } = null!;
    public string ApiKeyHash { get; set; } = null!;

    /// <summary>Comma- or space-separated scopes restricting which tools/resources are callable.</summary>
    public string? Scopes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastUsedAt { get; set; }
}
