using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
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
public class AccountingPeriodsController : ControllerBase
{
    private readonly IPeriodClosingService _closingService;

    public AccountingPeriodsController(IPeriodClosingService closingService)
    {
        _closingService = closingService;
    }

    [HttpPost("{id:guid}/validate-checklist")]
    [HasPermission(Permissions.FinancialReportsView)]
    public async Task<IActionResult> ValidateChecklist(Guid id)
    {
        var result = await _closingService.ValidateChecklistAsync(id);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/close")]
    [HasPermission(Permissions.AccountingManage)]
    public async Task<IActionResult> ClosePeriod(Guid id)
    {
        var userName = User.Identity?.Name ?? "System";
        var result = await _closingService.ClosePeriodAsync(id, userName);
        return Ok(new { success = true, data = result });
    }
}
