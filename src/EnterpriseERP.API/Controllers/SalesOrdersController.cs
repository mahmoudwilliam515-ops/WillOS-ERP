using EnterpriseERP.Application.Features.SalesOrders.Commands.CreateSalesOrder;
using EnterpriseERP.Application.Features.SalesOrders.Commands.ApproveSalesOrder;
using EnterpriseERP.Application.Features.SalesOrders.Queries.GetAllSalesOrders;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission(Permissions.SalesOrderView)]
public class SalesOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.SalesOrderView)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllSalesOrdersQuery(page, pageSize));
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.SalesOrderCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new 
        { 
            success = result.IsSuccess, 
            message = result.IsSuccess ? "Sales Order created successfully." : result.Error.Message,
            data = result.IsSuccess ? result.Value : Guid.Empty
        });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.SalesOrderManage)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveSalesOrderCommand(id));
        return Ok(new 
        { 
            success = result.IsSuccess, 
            message = result.IsSuccess ? "Sales Order approved and stock reserved." : result.Error.Message,
            data = result.IsSuccess
        });
    }
}
