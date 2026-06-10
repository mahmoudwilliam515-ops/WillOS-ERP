using EnterpriseERP.Application.Features.Warehouses.Commands.CreateWarehouse;
using EnterpriseERP.Application.Features.Warehouses.Queries.GetAllWarehouses;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.WarehousesView)]
    public async Task<IActionResult> GetAll()
    {
        var warehouses = await _mediator.Send(new GetAllWarehousesQuery());
        
        return Ok(new
        {
            Success = true,
            Message = "Warehouses retrieved successfully",
            Data = warehouses,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.WarehousesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseCommand command)
    {
        var warehouse = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetAll), new { id = warehouse.Id }, new
        {
            Success = true,
            Message = "Warehouse created successfully",
            Data = warehouse,
            Errors = Array.Empty<string>()
        });
    }
}
