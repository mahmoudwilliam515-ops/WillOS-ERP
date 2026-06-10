using EnterpriseERP.Application.Features.DeliveryNotes.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

/// <summary>
/// Delivery Notes Controller — إشعارات التسليم
/// Blueprint Section 1.2 — O2C-04
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DeliveryNotesController : ControllerBase
{
    private readonly ISender _sender;

    public DeliveryNotesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>POST /api/v1/delivery-notes — إنشاء إشعار تسليم + Fulfillment Gate</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeliveryNoteCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>GET /api/v1/delivery-notes/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeliveryNoteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken)
    {
        var query = new GetDeliveryNoteByIdQuery { DeliveryNoteId = id, CompanyId = companyId };
        var result = await _sender.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>GET /api/v1/delivery-notes — قائمة إشعارات التسليم</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? salesOrderId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new GetDeliveryNoteListQuery
        {
            CompanyId = companyId,
            SalesOrderId = salesOrderId,
            Status = status
        };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>PATCH /api/v1/delivery-notes/{id}/complete — إتمام التسليم</summary>
    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] CompleteDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteDeliveryNoteCommand
        {
            DeliveryNoteId = id,
            CompanyId = request.CompanyId,
            CompletedByUserId = request.CompletedByUserId
        };
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}

// ── DTOs & Queries ────────────────────────────────────────────────────────────

public record DeliveryNoteDetailDto
{
    public Guid Id { get; init; }
    public string NoteNumber { get; init; } = default!;
    public Guid SalesOrderId { get; init; }
    public string Status { get; init; } = default!;
    public DateTime DeliveryDate { get; init; }
    public List<DeliveryNoteLineDto> Lines { get; init; } = new();
}

public record DeliveryNoteLineDto
{
    public Guid SalesOrderLineId { get; init; }
    public Guid ItemId { get; init; }
    public decimal ShippedQuantity { get; init; }
}

public record CompleteDeliveryRequest
{
    public Guid CompanyId { get; init; }
    public Guid CompletedByUserId { get; init; }
}

public record GetDeliveryNoteByIdQuery : IRequest<DeliveryNoteDetailDto?>
{
    public Guid DeliveryNoteId { get; init; }
    public Guid CompanyId { get; init; }
}

public record GetDeliveryNoteListQuery : IRequest<IEnumerable<DeliveryNoteSummaryDto>>
{
    public Guid CompanyId { get; init; }
    public Guid? SalesOrderId { get; init; }
    public string? Status { get; init; }
}

public record DeliveryNoteSummaryDto
{
    public Guid Id { get; init; }
    public string NoteNumber { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime DeliveryDate { get; init; }
    public Guid SalesOrderId { get; init; }
}

public record CompleteDeliveryNoteCommand : IRequest<Unit>
{
    public Guid DeliveryNoteId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid CompletedByUserId { get; init; }
}
