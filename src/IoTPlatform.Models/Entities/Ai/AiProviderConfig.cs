using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Ai;

/// <summary>
/// Per-company AI provider configuration. The API key is stored encrypted at rest
/// (never in plaintext, never logged). Drives the provider-agnostic IChatClient.
/// </summary>
public class AiProviderConfig : TenantEntity
{
    public string Provider { get; set; } = null!; // e.g. OpenAI, AzureOpenAI, Anthropic, Ollama
    public string Model { get; set; } = null!;
    public string ApiKeyEncrypted { get; set; } = null!;
    public string? Endpoint { get; set; }
    public bool IsActive { get; set; } = true;
}
