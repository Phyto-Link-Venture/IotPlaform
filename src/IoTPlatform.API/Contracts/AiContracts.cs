namespace IoTPlatform.API.Contracts;

public sealed record ChatRequest(Guid? ConversationId, string Message);

public sealed record ChatResponseDto(
    Guid ConversationId,
    string Content,
    int? PromptTokens,
    int? CompletionTokens,
    string? Model);
