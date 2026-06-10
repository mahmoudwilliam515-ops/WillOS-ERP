using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Data;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Forensic audit trail - filterable by entity, action, date range
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.AuditLogsView)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _dbContext.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(l => l.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(l => l.Action == action);

        if (from.HasValue)
            query = query.Where(l => l.OccurredOn >= from.Value.ToUniversalTime());

        if (to.HasValue)
            query = query.Where(l => l.OccurredOn <= to.Value.ToUniversalTime());

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(l => l.OccurredOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                l.Id,
                l.EventType,
                l.EntityName,
                l.EntityId,
                l.Action,
                l.Details,
                l.OccurredOn,
                l.CreatedBy
            })
            .ToListAsync();

        return Ok(new
        {
            Success = true,
            Data = logs,
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Errors = Array.Empty<string>()
        });
    }

    /// <summary>
    /// Get distinct entity names for filtering
    /// </summary>
    [HttpGet("entity-types")]
    [HasPermission(Permissions.AuditLogsView)]
    public async Task<IActionResult> GetEntityTypes()
    {
        var types = await _dbContext.AuditLogs
            .Select(l => l.EntityName)
            .Distinct()
            .OrderBy(e => e)
            .ToListAsync();

        return Ok(new { Success = true, Data = types, Errors = Array.Empty<string>() });
    }
}
