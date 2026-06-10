using EnterpriseERP.Application.Features.Treasury.BankReconciliations.Commands.CreateBankReconciliation;
using EnterpriseERP.Application.Features.Treasury.BankReconciliations.Queries.GetAllBankReconciliations;
using EnterpriseERP.Application.Features.Treasury.BankReconciliations.Queries.GetUnreconciledTransactions;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BankReconciliationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BankReconciliationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.BankReconciliationView)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllBankReconciliationsQuery(page, pageSize));
        return Ok(new { success = true, data = result });
    }

    [HttpGet("unreconciled/{bankAccountId:guid}")]
    [HasPermission(Permissions.BankReconciliationView)]
    public async Task<IActionResult> GetUnreconciled(Guid bankAccountId)
    {
        var result = await _mediator.Send(new GetUnreconciledTransactionsQuery(bankAccountId));
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.BankReconciliationManage)]
    public async Task<IActionResult> Create([FromBody] CreateBankReconciliationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }
}
