using EnterpriseERP.Application.Features.SalesOrders.Commands;
using EnterpriseERP.Application.Features.SalesOrders.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

/// <summary>
/// Sales Orders Controller — أوامر البيع
/// Blueprint Section 1.2 — O2C
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public SalesOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>POST /api/v1/sales-orders — إنشاء أمر بيع جديد</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesOrderCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>GET /api/v1/sales-orders/{id} — جلب أمر بيع محدد</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SalesOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken)
    {
        var query = new GetSalesOrderByIdQuery { SalesOrderId = id, CompanyId = companyId };
        var result = await _sender.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>GET /api/v1/sales-orders — قائمة أوامر البيع</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SalesOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? customerId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetSalesOrderListQuery
        {
            CompanyId = companyId,
            CustomerId = customerId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>PATCH /api/v1/sales-orders/{id}/confirm — تأكيد أمر البيع + Credit Check + Reservation</summary>
    [HttpPatch("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirm(
        Guid id,
        [FromBody] ConfirmSalesOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmSalesOrderCommand
        {
            SalesOrderId = id,
            CompanyId = request.CompanyId,
            ConfirmedByUserId = request.ConfirmedByUserId
        };
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>PATCH /api/v1/sales-orders/{id}/ship — ترحيل أمر البيع إلى Shipped</summary>
    [HttpPatch("{id:guid}/ship")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ship(
        Guid id,
        [FromBody] ShipSalesOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ShipSalesOrderCommand
        {
            SalesOrderId = id,
            CompanyId = request.CompanyId,
            DeliveryNoteId = request.DeliveryNoteId
        };
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record ConfirmSalesOrderRequest
{
    public Guid CompanyId { get; init; }
    public Guid ConfirmedByUserId { get; init; }
}

public record ShipSalesOrderRequest
{
    public Guid CompanyId { get; init; }
    public Guid DeliveryNoteId { get; init; }
}

public record SalesOrderSummaryDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = default!;
    public string CustomerName { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime OrderDate { get; init; }
    public decimal TotalAmount { get; init; }
    public bool CreditCheckPassed { get; init; }
}

public record GetSalesOrderListQuery : IRequest<IEnumerable<SalesOrderSummaryDto>>
{
    public Guid CompanyId { get; init; }
    public Guid? CustomerId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
