using EnterpriseERP.Application.Features.PurchaseReturns.Commands.ApproveDebitNote;
using EnterpriseERP.Application.Features.PurchaseReturns.Commands.CreateDebitNote;
using EnterpriseERP.Application.Features.PurchaseReturns.Queries.GetAllDebitNotes;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DebitNotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DebitNotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.PurchaseReturnsView)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllDebitNotesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.PurchaseReturnsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateDebitNoteCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.IsSuccess ? result.Value : Guid.Empty, message = result.IsSuccess ? "" : result.Error.Message });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.PurchaseReturnsApprove)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveDebitNoteCommand(id));
        return Ok(new { success = result.IsSuccess, message = result.IsSuccess ? "Debit Note approved." : result.Error.Message });
    }
}
