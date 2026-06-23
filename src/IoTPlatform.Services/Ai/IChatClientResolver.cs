using Microsoft.Extensions.AI;

namespace IoTPlatform.Services.Ai;

/// <summary>
/// Resolves a provider-agnostic <see cref="IChatClient"/> for the current tenant,
/// based on its active <c>ai_provider_configs</c> row (provider, model, decrypted key).
/// </summary>
public interface IChatClientResolver
{
    Task<IChatClient> ResolveAsync(CancellationToken cancellationToken = default);
}
