namespace IoTPlatform.Models.Common;

/// <summary>
/// Base class for tenant-scoped entities. The <see cref="CompanyId"/> drives the
/// multi-tenant global query filter so data is isolated per company.
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public Guid CompanyId { get; set; }
}
