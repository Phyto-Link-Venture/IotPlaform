namespace IoTPlatform.Models.Common;

/// <summary>
/// Base class for every entity. Provides identity, audit trail, and soft-delete
/// fields. Audit fields are populated automatically by the persistence interceptor.
/// </summary>
public abstract class AuditableEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
