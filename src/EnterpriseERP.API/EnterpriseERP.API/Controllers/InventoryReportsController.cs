using EnterpriseERP.Application.Features.Inventory.Queries.GetItemLedger;
using EnterpriseERP.Application.Features.Inventory.Queries.GetStockBalance;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stock-balance")]
    [HasPermission(Permissions.InventoryReportsView)]
    public async Task<IActionResult> GetStockBalance([FromQuery] GetStockBalanceQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new
        {
            Success = true,
            Message = "Stock balance retrieved successfully",
            Data = result,
            Errors = Array.Empty<string>()
        });
    }

    [HttpGet("item-ledger")]
    [HasPermission(Permissions.InventoryReportsView)]
    public async Task<IActionResult> GetItemLedger([FromQuery] GetItemLedgerQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new
        {
            Success = true,
            Message = "Item ledger retrieved successfully",
            Data = result,
            Errors = Array.Empty<string>()
        });
    }
}
