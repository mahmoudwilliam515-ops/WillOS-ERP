using EnterpriseERP.Application.Features.Logistics.DeliveryNotes.Commands.CreateDeliveryNote;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.ApproveGRN;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.CreateGRN;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using EnterpriseERP.Domain.Entities.Identity;

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

    [HttpPost("grn")]
    [HasPermission(Permissions.LogisticsManage)]
    public async Task<IActionResult> CreateGoodsReceiptNote([FromBody] CreateGRNCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.IsSuccess ? result.Value : null, error = result.Error });
    }

    [HttpPost("grn/{id:guid}/approve")]
    [HasPermission(Permissions.LogisticsManage)]
    public async Task<IActionResult> ApproveGoodsReceiptNote(Guid id, [FromHeader] Guid companyId, [FromHeader] Guid userId)
    {
        var result = await _mediator.Send(new ApproveGRNCommand 
        { 
            GRNId = id, 
            CompanyId = companyId, 
            ApprovedByUserId = userId 
        });
        
        return Ok(new { success = result.IsSuccess, error = result.Error });
    }

    [HttpPost("delivery-note")]
    [HasPermission(Permissions.LogisticsManage)]
    public async Task<IActionResult> CreateDeliveryNote([FromBody] CreateDeliveryNoteCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.IsSuccess ? result.Value : null, error = result.Error });
    }
}
