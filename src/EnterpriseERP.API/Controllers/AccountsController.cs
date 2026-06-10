using EnterpriseERP.Application.Features.Accounting.Commands.CreateAccount;
using EnterpriseERP.Application.Features.Accounting.Commands.CreateFiscalYear;
using EnterpriseERP.Application.Features.Accounting.Commands.CreateCostCenter;
using EnterpriseERP.Application.Features.Accounting.Queries.GetAccounts;
using EnterpriseERP.Application.Features.Accounting.Queries.GetCostCenters;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HasPermission(Permissions.AccountsCreate)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value, message = "Account created." });
            
        return BadRequest(new { success = false, message = result.Error.Message });
    }

    [HttpGet]
    [HasPermission(Permissions.AccountsView)]
    public async Task<IActionResult> GetAccounts([FromQuery] bool? isActive, [FromQuery] int? type, [FromQuery] System.Guid? parentId)
    {
        var result = await _mediator.Send(new GetAccountsQuery { IsActive = isActive, Type = type, ParentId = parentId });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost("fiscal-years")]
    [HasPermission(Permissions.AccountsCreate)]
    public async Task<IActionResult> CreateFiscalYear([FromBody] CreateFiscalYearCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value, message = "Fiscal year created." });
            
        return BadRequest(new { success = false, message = result.Error.Message });
    }

    [HttpGet("cost-centers")]
    [HasPermission(Permissions.AccountsView)]
    public async Task<IActionResult> GetCostCenters([FromQuery] bool? isActive)
    {
        var result = await _mediator.Send(new GetCostCentersQuery { IsActive = isActive });
        return Ok(new { success = true, data = result.Value });
    }

    [HttpPost("cost-centers")]
    [HasPermission(Permissions.AccountsCreate)]
    public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value, message = "Cost center created." });
            
        return BadRequest(new { success = false, message = result.Error.Message });
    }

    [HttpPost("periods/{id:guid}/close")]
    [HasPermission(Permissions.AccountsCreate)] // Using same permission or maybe specific AccountsManage
    public async Task<IActionResult> ClosePeriod(System.Guid id)
    {
        var command = new EnterpriseERP.Application.Features.Accounting.Commands.CloseAccountingPeriod.CloseAccountingPeriodCommand { PeriodId = id };
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value, message = "Accounting period closed." });
            
        return BadRequest(new { success = false, message = result.Error });
    }
}
