using EnterpriseERP.Application.Features.Accounting.Commands.CloseFiscalYear;
using EnterpriseERP.Application.Features.Accounting.FiscalYears.Commands.CloseAccountingPeriod;
using EnterpriseERP.Application.Features.Accounting.FiscalYears.Commands.CreateFiscalYear;
using EnterpriseERP.Application.Features.Accounting.FiscalYears.Queries.GetAllFiscalYears;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FiscalYearsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FiscalYearsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.FiscalYearsView)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllFiscalYearsQuery());
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.FiscalYearsManage)]
    public async Task<IActionResult> Create([FromBody] CreateFiscalYearCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("periods/{id}/close")]
    [HasPermission(Permissions.FiscalYearsManage)]
    public async Task<IActionResult> ClosePeriod(Guid id)
    {
        var command = new CloseAccountingPeriodCommand { PeriodId = id };
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/close")]
    [HasPermission(Permissions.FiscalYearsManage)]
    public async Task<IActionResult> CloseFiscalYear(Guid id)
    {
        var result = await _mediator.Send(new CloseFiscalYearCommand(id));
        return Ok(new { success = result, message = result ? "Fiscal year closed and closing journal entry posted." : "Year not found or already closed." });
    }
}
