namespace IoTPlatform.Common.Abstractions;

/// <summary>
/// Resolves the active tenant (company) and optional department scope for the
/// current request. Drives the multi-tenant global query filter. SuperAdmin
/// requests may set <see cref="IgnoreTenantFilter"/> to query across tenants.
/// </summary>
public interface ITenantContext
{
    Guid? CompanyId { get; }
    Guid? DepartmentId { get; }
    bool IgnoreTenantFilter { get; }
}
