using IoTPlatform.Common.Results;

namespace IoTPlatform.Services.Ai;

public interface IAiAssistantService
{
    /// <summary>
    /// Sends a user message to the tenant's configured LLM (with platform tools available),
    /// persists the full exchange, and audits the invocation. Creates a new conversation
    /// when <paramref name="conversationId"/> is null.
    /// </summary>
    Task<Result<AiReply>> ChatAsync(
        Guid? conversationId,
        string userMessage,
        CancellationToken cancellationToken = default);
}

public sealed record AiReply(Guid ConversationId, string Content, int? PromptTokens, int? CompletionTokens, string? Model);
