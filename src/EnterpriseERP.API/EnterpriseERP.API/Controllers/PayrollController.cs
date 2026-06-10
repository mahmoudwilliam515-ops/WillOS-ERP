using EnterpriseERP.Application.Features.HR.Commands.ProcessPayroll;
using EnterpriseERP.Application.Features.HR.Queries.GetPayrollHistory;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("history")]
    [HasPermission(Permissions.PayrollView)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetPayrollHistoryQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Year = year,
            Month = month
        });
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("process")]
    [HasPermission(Permissions.PayrollProcess)]
    public async Task<IActionResult> ProcessPayroll([FromBody] ProcessPayrollCommand command)
    {
        var journalEntryId = await _mediator.Send(command);
        return Ok(new { Success = true, Data = journalEntryId, Message = "Payroll processed successfully and Journal Entry created." });
    }
}
