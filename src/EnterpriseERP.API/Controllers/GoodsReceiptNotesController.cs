using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.CreateGoodsReceiptNote;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.ApproveGoodsReceiptNote;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Queries.GetAllGoodsReceiptNotes;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoodsReceiptNotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GoodsReceiptNotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetAllGoodsReceiptNotesQuery { PageNumber = pageNumber, PageSize = pageSize });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> Create([FromBody] CreateGoodsReceiptNoteCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new 
        { 
            success = result.IsSuccess, 
            message = result.IsSuccess ? "GRN created successfully." : result.Error.Message,
            data = result.IsSuccess ? result.Value : Guid.Empty 
        });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveGoodsReceiptNoteCommand(id));
        return Ok(new { success = result, message = result ? "GRN approved and inventory updated." : "Approval failed." });
    }
}
