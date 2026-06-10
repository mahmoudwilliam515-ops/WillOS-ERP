using EnterpriseERP.Application.Features.Logistics.DeliveryNotes.Commands.CreateDeliveryNote;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.ApproveGoodsReceiptNote;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.CreateGoodsReceiptNote;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Queries.GetAllGoodsReceiptNotes;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LogisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("grn")]
    [HasPermission(Permissions.LogisticsView)]
    public async Task<IActionResult> GetAllGoodsReceiptNotes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllGoodsReceiptNotesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });
        return Ok(new { success = true, data = result });
    }

    [HttpPost("grn")]
    [HasPermission(Permissions.LogisticsManage)]
    public async Task<IActionResult> CreateGoodsReceiptNote([FromBody] CreateGoodsReceiptNoteCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("grn/{id:guid}/approve")]
    [HasPermission(Permissions.LogisticsManage)]
    public async Task<IActionResult> ApproveGoodsReceiptNote(Guid id)
    {
        var ok = await _mediator.Send(new ApproveGoodsReceiptNoteCommand(id));
        if (!ok)
            return NotFound(new { success = false, message = "GRN not found, already received, or has no lines." });
        return Ok(new { success = true });
    }

    [HttpPost("delivery-note")]
    [HasPermission(Permissions.LogisticsManage)]
    public async Task<IActionResult> CreateDeliveryNote([FromBody] CreateDeliveryNoteCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }
}
