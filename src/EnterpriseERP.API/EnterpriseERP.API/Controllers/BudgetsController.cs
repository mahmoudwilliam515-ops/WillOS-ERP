using EnterpriseERP.Application.Features.Accounting.Budgets.Commands.CreateBudget;
using EnterpriseERP.Application.Features.Accounting.Budgets.Queries.GetBudgetVsActual;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Create a new budget for a fiscal year.</summary>
    [HttpPost]
    [HasPermission(Permissions.BudgetsManage)]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetBudgetVsActual), new { id = result.Value.Id },
                new { success = true, data = result.Value, message = "Budget created." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }

    /// <summary>Budget vs Actual variance report for a specific budget.</summary>
    [HttpGet("{id:guid}/vs-actual")]
    [HasPermission(Permissions.BudgetsView)]
    public async Task<IActionResult> GetBudgetVsActual(Guid id)
    {
        var result = await _mediator.Send(new GetBudgetVsActualQuery { BudgetId = id });
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value });

        return BadRequest(new { success = false, message = result.Error.Message });
    }
}
