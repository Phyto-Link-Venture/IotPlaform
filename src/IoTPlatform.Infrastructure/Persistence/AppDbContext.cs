using System.Linq.Expressions;
using IoTPlatform.Common.Abstractions;
using IoTPlatform.Models.Common;
using IoTPlatform.Models.Entities.Access;
using IoTPlatform.Models.Entities.Ai;
using IoTPlatform.Models.Entities.Iot;
using IoTPlatform.Models.Entities.Logging;
using IoTPlatform.Models.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.Infrastructure.Persistence;

/// <summary>
/// EF Core context. Applies global query filters for soft delete and multi-tenancy
/// (company scope) across all relevant entities. snake_case naming is configured at
/// registration time via UseSnakeCaseNamingConvention.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    // Tenancy
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserGroupMember> UserGroupMembers => Set<UserGroupMember>();

    // Access control
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserGroupPermission> UserGroupPermissions => Set<UserGroupPermission>();

    // IoT
    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<TelemetryData> TelemetryData => Set<TelemetryData>();
    public DbSet<DeviceCommand> DeviceCommands => Set<DeviceCommand>();

    // AI & MCP
    public DbSet<AiProviderConfig> AiProviderConfigs => Set<AiProviderConfig>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public DbSet<McpClient> McpClients => Set<McpClient>();

    // Logging
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<> in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ApplyGlobalQueryFilters(modelBuilder);
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");
            Expression? body = null;

            // Soft-delete filter for auditable entities.
            if (typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                var isDeleted = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
                body = Expression.Not(isDeleted);
            }

            // Tenant filter for tenant-scoped entities (bypassed for SuperAdmin).
            if (typeof(TenantEntity).IsAssignableFrom(clrType))
            {
                var companyId = Expression.Property(parameter, nameof(TenantEntity.CompanyId));
                // e.CompanyId == tenantContext.CompanyId  (only when a tenant is set and filter is not ignored)
                var tenantProp = Expression.Property(
                    Expression.Constant(this), nameof(AppDbContext.TenantContext));
                var ignoreProp = Expression.Property(tenantProp, nameof(ITenantContext.IgnoreTenantFilter));
                var ctxCompanyId = Expression.Property(tenantProp, nameof(ITenantContext.CompanyId));

                var nullableCompanyId = Expression.Convert(companyId, typeof(Guid?));
                var tenantMatch = Expression.OrElse(
                    ignoreProp,
                    Expression.OrElse(
                        Expression.Equal(ctxCompanyId, Expression.Constant(null, typeof(Guid?))),
                        Expression.Equal(nullableCompanyId, ctxCompanyId)));

                body = body is null ? tenantMatch : Expression.AndAlso(body, tenantMatch);
            }

            if (body is not null)
                modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(body, parameter));
        }
    }

    /// <summary>Exposed so the query-filter expression tree can read the live tenant context.</summary>
    public ITenantContext TenantContext => tenantContext;
}
