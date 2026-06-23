using IoTPlatform.Common.Abstractions;

namespace IoTPlatform.API.Authentication;

/// <summary>
/// Resolves the active tenant scope for the request. The user's own company comes from the
/// JWT; the frontend "global filter" may narrow scope via the X-Company-Id / X-Department-Id
/// headers. Narrowing only — a non-superadmin can never select a company other than their own.
/// A superadmin with no company selected gets a cross-tenant view (filter ignored).
/// </summary>
public sealed class TenantContext : ITenantContext
{
    public TenantContext(IHttpContextAccessor accessor, ICurrentUser currentUser)
    {
        var http = accessor.HttpContext;

        Guid? ownCompany =
            Guid.TryParse(http?.User.FindFirst(AppClaimTypes.CompanyId)?.Value, out var c) ? c : null;

        Guid? requestedCompany =
            Guid.TryParse(http?.Request.Headers["X-Company-Id"], out var rc) ? rc : null;
        Guid? requestedDepartment =
            Guid.TryParse(http?.Request.Headers["X-Department-Id"], out var rd) ? rd : null;

        if (currentUser.IsSuperAdmin)
        {
            CompanyId = requestedCompany;                 // may be null => see all tenants
            IgnoreTenantFilter = requestedCompany is null;
        }
        else
        {
            // Non-superadmins are pinned to their own company regardless of the requested header.
            CompanyId = ownCompany;
            IgnoreTenantFilter = false;
        }

        DepartmentId = requestedDepartment;
    }

    public Guid? CompanyId { get; }
    public Guid? DepartmentId { get; }
    public bool IgnoreTenantFilter { get; }
}
