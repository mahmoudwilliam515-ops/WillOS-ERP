using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GovernanceController : ControllerBase
{
    private readonly IAIGovernanceService _aiGovernanceService;

    public GovernanceController(IAIGovernanceService aiGovernanceService)
    {
        _aiGovernanceService = aiGovernanceService;
    }

    [HttpGet("anomalies")]
    [HasPermission(Permissions.JournalEntriesView)] // High level permission for now
    public async Task<IActionResult> GetAnomalies([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        if (fromDate == default) fromDate = DateTime.UtcNow.AddMonths(-1);
        if (toDate == default) toDate = DateTime.UtcNow;

        var result = await _aiGovernanceService.ScanForAnomaliesAsync(fromDate, toDate);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("analyze/{id:guid}")]
    [HasPermission(Permissions.JournalEntriesView)]
    public async Task<IActionResult> AnalyzeEntry(Guid id)
    {
        var result = await _aiGovernanceService.AnalyzeJournalEntryAsync(id);
        return Ok(new { Success = true, Data = result });
    }
}
