using EnterpriseERP.Application.Features.Intercompany.Commands.CreateIntercompanyTransaction;
using EnterpriseERP.Application.Features.Intercompany.Commands.MatchIntercompanyTransaction;
using EnterpriseERP.Application.Features.Intercompany.Commands.RunConsolidation;
using EnterpriseERP.Application.Features.Intercompany.Queries.GetIntercompanyTransactions;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntercompanyController(IMediator mediator) : ControllerBase
{
    [HttpGet("transactions")]
    [HasPermission(Permissions.IntercompanyView)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] IntercompanyTransactionStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var transactions = await mediator.Send(new GetIntercompanyTransactionsQuery
        {
            Status = status,
            From = from,
            To = to
        });

        return Ok(new { Success = true, Data = transactions });
    }

    [HttpPost("transactions")]
    [HasPermission(Permissions.IntercompanyManage)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateIntercompanyTransactionCommand command)
    {
        var transaction = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, new
        {
            Success = true,
            Message = "Intercompany transaction created successfully",
            Data = transaction,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost("transactions/{id:guid}/match")]
    [HasPermission(Permissions.IntercompanyManage)]
    public async Task<IActionResult> MatchTransaction(Guid id)
    {
        var result = await mediator.Send(new MatchIntercompanyTransactionCommand(id));
        return Ok(new { Success = result });
    }

    [HttpPost("consolidation-runs")]
    [HasPermission(Permissions.IntercompanyManage)]
    public async Task<IActionResult> RunConsolidation([FromBody] RunConsolidationCommand command)
    {
        var run = await mediator.Send(command);
        return Ok(new
        {
            Success = true,
            Message = "Consolidation run completed successfully",
            Data = run,
            Errors = Array.Empty<string>()
        });
    }
}
