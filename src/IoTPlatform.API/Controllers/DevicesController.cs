using IoTPlatform.API.Authorization;
using IoTPlatform.API.Contracts;
using IoTPlatform.Common.Constants;
using IoTPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoTPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DevicesController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Lists devices visible to the caller. The multi-tenant query filter scopes to the
    /// active company automatically; department scope is applied on top when selected.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Devices.List)]
    public async Task<ActionResult<IReadOnlyList<DeviceDto>>> List(CancellationToken cancellationToken)
    {
        var departmentId = db.TenantContext.DepartmentId;

        var devices = await db.Devices
            .AsNoTracking()
            .Where(d => departmentId == null || d.DepartmentId == departmentId)
            .OrderBy(d => d.Name)
            .Select(d => new DeviceDto(
                d.Id, d.DeviceKey, d.Name, d.DeviceType.Name, d.Status, d.LastSeenAt, d.DepartmentId))
            .ToListAsync(cancellationToken);

        return Ok(devices);
    }
}
