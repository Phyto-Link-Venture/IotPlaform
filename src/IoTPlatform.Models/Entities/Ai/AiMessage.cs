using IoTPlatform.Models.Common;
using IoTPlatform.Models.Enums;

namespace IoTPlatform.Models.Entities.Ai;

/// <summary>
/// One message in a conversation. Tool calls and token usage are persisted so all
/// LLM activity is fully auditable (see also AuditLog / SystemLog).
/// </summary>
public class AiMessage : AuditableEntity
{
    public Guid ConversationId { get; set; }
    public AiMessageRole Role { get; set; }
    public string Content { get; set; } = null!;

    /// <summary>Serialized tool/function calls (jsonb), if any.</summary>
    public string? ToolCalls { get; set; }

    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public string? Model { get; set; }

    public AiConversation Conversation { get; set; } = null!;
}
