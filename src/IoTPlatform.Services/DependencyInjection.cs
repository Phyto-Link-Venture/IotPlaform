using IoTPlatform.Services.Ai;
using IoTPlatform.Services.Ai.Tools;
using IoTPlatform.Services.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace IoTPlatform.Services;

public static class DependencyInjection
{
    /// <summary>Registers business services: authorization, AI assistant, and AI tools.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<DeviceAiTools>();
        services.AddScoped<IChatClientResolver, ChatClientResolver>();
        services.AddScoped<IAiAssistantService, AiAssistantService>();

        return services;
    }
}
