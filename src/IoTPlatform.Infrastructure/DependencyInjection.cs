using IoTPlatform.Common.Abstractions;
using IoTPlatform.Infrastructure.Messaging.Mqtt;
using IoTPlatform.Infrastructure.Persistence;
using IoTPlatform.Infrastructure.Persistence.Interceptors;
using IoTPlatform.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IoTPlatform.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence (PostgreSQL + snake_case + audit interceptor + query filters),
    /// the key protector, and MQTT ingestion/publishing.
    /// Note: <see cref="ICurrentUser"/> and <see cref="ITenantContext"/> are supplied by the API layer.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(configuration.GetConnectionString("Postgres"))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        // AI provider API-key encryption at rest (Data Protection; swappable for KMS later).
        services.AddDataProtection();
        services.AddSingleton<IKeyProtector, DataProtectionKeyProtector>();

        // MQTT: single client instance shared as hosted service + publisher.
        services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));
        services.AddScoped<ITelemetryIngestor, TelemetryIngestor>();
        services.AddSingleton<MqttBackgroundService>();
        services.AddSingleton<IMqttPublisher>(sp => sp.GetRequiredService<MqttBackgroundService>());
        services.AddHostedService(sp => sp.GetRequiredService<MqttBackgroundService>());

        return services;
    }
}
