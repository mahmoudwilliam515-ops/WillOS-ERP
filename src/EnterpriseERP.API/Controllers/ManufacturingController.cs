using System;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.Manufacturing.Commands.CreateProductionOrder;
using EnterpriseERP.Application.Features.Manufacturing.Commands.CompleteProductionOrder;
using EnterpriseERP.Application.Features.Manufacturing.Commands.StartProductionOrder;
using EnterpriseERP.Application.Features.Manufacturing.Queries.GetProductionOrders;
using EnterpriseERP.Application.Features.Manufacturing.Queries.GetManufacturingKpis;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using EnterpriseERP.Application.Features.Manufacturing.Queries.GetProductionProfitabilityReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingController : ControllerBase
{
    private readonly IMrpService _mrpService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public ManufacturingController(IMrpService mrpService, ICurrentUserService currentUserService, IMediator mediator)
    {
        _mrpService = mrpService;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    [HttpGet("orders")]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetOrders([FromQuery] GetProductionOrdersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { success = result.IsSuccess, data = result.Value });
    }

    [HttpPost("orders")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateProductionOrderCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.Value, message = result.IsSuccess ? "Order created" : result.Error.Message });
    }

    [HttpPost("orders/{id:guid}/start")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> StartOrder(Guid id)
    {
        var result = await _mediator.Send(new StartProductionOrderCommand { ProductionOrderId = id });
        return Ok(new { success = result.IsSuccess, message = result.IsSuccess ? "Order started" : result.Error.Message });
    }

    [HttpPost("orders/{id:guid}/complete")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> CompleteOrder(Guid id, [FromBody] CompleteProductionOrderCommand command)
    {
        if (id != command.ProductionOrderId) return BadRequest();
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, message = result.IsSuccess ? "Order completed and inventory updated" : result.Error.Message });
    }

    [HttpGet("kpis")]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetManufacturingKpis()
    {
        var result = await _mediator.Send(new GetManufacturingKpisQuery());
        return Ok(new { success = result.IsSuccess, data = result.Value });
    }

    [HttpGet("mrp/recommendations")]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetMrpRecommendations([FromQuery] DateTime? forecastEndDate)
    {
        var companyId = _currentUserService.CompanyId;
        if (companyId == null || companyId == Guid.Empty)
            return BadRequest(new { success = false, message = "Company context is required." });

        var date = forecastEndDate ?? DateTime.UtcNow.AddMonths(1);
        var result = await _mrpService.RunMrpAsync(companyId.Value, date);
        
        return Ok(new { success = true, data = result });
    }

    [HttpGet("work-centers/{id:guid}/availability")]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetWorkCenterAvailability(Guid id, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var scheduler = HttpContext.RequestServices.GetRequiredService<IProductionSchedulerService>();
        var result = await scheduler.GetWorkCenterAvailabilityAsync(id, startDate, endDate);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("reports/profitability")]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetProfitabilityReport([FromQuery] GetProductionProfitabilityReportQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { success = true, data = result });
    }
}
