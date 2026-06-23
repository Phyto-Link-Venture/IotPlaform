using System.ClientModel;
using IoTPlatform.Common.Abstractions;
using IoTPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;

namespace IoTPlatform.Services.Ai;

/// <summary>
/// Builds an <see cref="IChatClient"/> from the active tenant's <c>ai_provider_configs</c> row.
/// Supports OpenAI and any OpenAI-compatible endpoint (Azure OpenAI, Ollama, local gateways)
/// out of the box. To add a native provider (e.g. Anthropic), reference its
/// Microsoft.Extensions.AI integration package and extend the switch below.
/// </summary>
public sealed class ChatClientResolver(
    AppDbContext db,
    ITenantContext tenantContext,
    IKeyProtector keyProtector) : IChatClientResolver
{
    public async Task<IChatClient> ResolveAsync(CancellationToken cancellationToken = default)
    {
        if (tenantContext.CompanyId is not { } companyId)
            throw new InvalidOperationException("No active tenant; cannot resolve an AI client.");

        var config = await db.AiProviderConfigs.AsNoTracking()
            .Where(c => c.CompanyId == companyId && c.IsActive)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active AI provider configured for this company.");

        var apiKey = keyProtector.Unprotect(config.ApiKeyEncrypted);

        return config.Provider.Trim().ToLowerInvariant() switch
        {
            "openai" or "openai-compatible" or "azureopenai" or "ollama"
                => BuildOpenAiCompatible(apiKey, config.Model, config.Endpoint),

            var other => throw new NotSupportedException(
                $"AI provider '{other}' is not wired up. Add its Microsoft.Extensions.AI " +
                $"integration package and extend {nameof(ChatClientResolver)}."),
        };
    }

    private static IChatClient BuildOpenAiCompatible(string apiKey, string model, string? endpoint)
    {
        var options = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(endpoint))
            options.Endpoint = new Uri(endpoint);

        var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        return client.GetChatClient(model).AsIChatClient();
    }
}
