using System.Text.Json;
using IoTPlatform.Common.Abstractions;
using IoTPlatform.Common.Results;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Models.Entities.Ai;
using IoTPlatform.Models.Entities.Logging;
using IoTPlatform.Models.Enums;
using IoTPlatform.Services.Ai.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace IoTPlatform.Services.Ai;

/// <summary>
/// Orchestrates a chat turn: resolves the tenant's LLM, exposes platform tools via
/// function calling, persists the full exchange (with token usage) to ai_messages, and
/// writes an audit record. All LLM activity is therefore double-logged (ai_messages + audit_logs).
/// </summary>
public sealed class AiAssistantService(
    IChatClientResolver clientResolver,
    DeviceAiTools deviceTools,
    AppDbContext db,
    ICurrentUser currentUser,
    ITenantContext tenantContext) : IAiAssistantService
{
    private const string SystemPrompt =
        "You are the IoTPlatform assistant. Help users understand their devices and telemetry. " +
        "Use the provided tools to look up live data instead of guessing.";

    public async Task<Result<AiReply>> ChatAsync(
        Guid? conversationId, string userMessage, CancellationToken cancellationToken = default)
    {
        if (tenantContext.CompanyId is not { } companyId)
            return Error.Forbidden("No active tenant context for AI chat.");
        if (currentUser.UserId is not { } userId)
            return Error.Unauthorized("Authentication required for AI chat.");

        var conversation = await GetOrCreateConversationAsync(conversationId, companyId, userId, cancellationToken);

        // Build the message window: system prompt + prior turns + new user message.
        var messages = new List<ChatMessage> { new(ChatRole.System, SystemPrompt) };
        var history = await db.AiMessages.AsNoTracking()
            .Where(m => m.ConversationId == conversation.Id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
        messages.AddRange(history.Select(m => new ChatMessage(MapRole(m.Role), m.Content)));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));

        var baseClient = await clientResolver.ResolveAsync(cancellationToken);
        using var client = baseClient.AsBuilder().UseFunctionInvocation().Build();

        var options = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(deviceTools.GetDeviceStatusAsync),
                AIFunctionFactory.Create(deviceTools.QueryRecentTelemetryAsync),
            ],
        };

        var response = await client.GetResponseAsync(messages, options, cancellationToken);
        var assistantText = response.Text ?? string.Empty;
        var promptTokens = (int?)response.Usage?.InputTokenCount;
        var completionTokens = (int?)response.Usage?.OutputTokenCount;

        // Persist user + assistant messages.
        db.AiMessages.Add(new AiMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = AiMessageRole.User,
            Content = userMessage,
        });
        db.AiMessages.Add(new AiMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = AiMessageRole.Assistant,
            Content = assistantText,
            Model = response.ModelId,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
        });

        // Audit the LLM invocation.
        db.AuditLogs.Add(new AuditLog
        {
            CompanyId = companyId,
            UserId = userId,
            EntityType = nameof(AiConversation),
            EntityId = conversation.Id.ToString(),
            Action = "AiInvoke",
            NewValue = JsonSerializer.Serialize(new { response.ModelId, promptTokens, completionTokens }),
            Timestamp = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync(cancellationToken);

        return new AiReply(conversation.Id, assistantText, promptTokens, completionTokens, response.ModelId);
    }

    private async Task<AiConversation> GetOrCreateConversationAsync(
        Guid? conversationId, Guid companyId, Guid userId, CancellationToken cancellationToken)
    {
        if (conversationId is { } id)
        {
            var existing = await db.AiConversations.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (existing is not null)
                return existing;
        }

        var conversation = new AiConversation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            UserId = userId,
        };
        db.AiConversations.Add(conversation);
        return conversation;
    }

    private static ChatRole MapRole(AiMessageRole role) => role switch
    {
        AiMessageRole.System => ChatRole.System,
        AiMessageRole.User => ChatRole.User,
        AiMessageRole.Assistant => ChatRole.Assistant,
        AiMessageRole.Tool => ChatRole.Tool,
        _ => ChatRole.User,
    };
}
