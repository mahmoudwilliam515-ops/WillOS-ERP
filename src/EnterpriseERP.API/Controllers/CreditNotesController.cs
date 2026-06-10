using EnterpriseERP.Application.Features.SalesReturns.Commands.ApproveCreditNote;
using EnterpriseERP.Application.Features.SalesReturns.Commands.CreateCreditNote;
using EnterpriseERP.Application.Features.SalesReturns.Queries.GetAllCreditNotes;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CreditNotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CreditNotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.SalesReturnsView)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllCreditNotesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.SalesReturnsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateCreditNoteCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.IsSuccess ? result.Value : Guid.Empty, message = result.IsSuccess ? "" : result.Error.Message });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.SalesReturnsApprove)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveCreditNoteCommand(id));
        return Ok(new { success = result.IsSuccess, message = result.IsSuccess ? "Credit Note approved." : result.Error.Message });
    }
}
