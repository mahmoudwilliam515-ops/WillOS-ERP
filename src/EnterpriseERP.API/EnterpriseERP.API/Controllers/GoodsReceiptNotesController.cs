using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.CreateGRN;
using EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.ApproveGRN;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

/// <summary>
/// GRN Controller â€” Ø³Ù†Ø¯Ø§Øª Ø§Ø³ØªÙ„Ø§Ù… Ø§Ù„Ø¨Ø¶Ø§Ø¹Ø©
/// Blueprint Section 1.1 â€” P2P-03
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class GoodsReceiptNotesController : ControllerBase
{
    private readonly ISender _sender;

    public GoodsReceiptNotesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>POST /api/v1/goods-receipt-notes â€” Ø¥Ù†Ø´Ø§Ø¡ Ø³Ù†Ø¯ Ø§Ø³ØªÙ„Ø§Ù… Ø¬Ø¯ÙŠØ¯</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGRNCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>GET /api/v1/goods-receipt-notes/{id} â€” Ø¬Ù„Ø¨ Ø³Ù†Ø¯ Ø§Ø³ØªÙ„Ø§Ù… Ù…Ø­Ø¯Ø¯</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GRNDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken)
    {
        var query = new GetGRNByIdQuery { GRNId = id, CompanyId = companyId };
        var result = await _sender.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>GET /api/v1/goods-receipt-notes â€” Ù‚Ø§Ø¦Ù…Ø© Ø³Ù†Ø¯Ø§Øª Ø§Ù„Ø§Ø³ØªÙ„Ø§Ù…</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GRNSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? purchaseOrderId,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetGRNListQuery
        {
            CompanyId = companyId,
            PurchaseOrderId = purchaseOrderId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>PATCH /api/v1/goods-receipt-notes/{id}/approve â€” Ø§Ø¹ØªÙ…Ø§Ø¯ Ø³Ù†Ø¯ Ø§Ù„Ø§Ø³ØªÙ„Ø§Ù…</summary>
    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveGRNRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveGRNCommand
        {
            GRNId = id,
            CompanyId = request.CompanyId,
            ApprovedByUserId = request.ApprovedByUserId
        };
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}

// â”€â”€ DTOs â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

public record ApproveGRNRequest
{
    public Guid CompanyId { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

public record GRNSummaryDto
{
    public Guid Id { get; init; }
    public string GRNNumber { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime ReceiptDate { get; init; }
    public Guid PurchaseOrderId { get; init; }
    public decimal TotalCost { get; init; }
    public int LinesCount { get; init; }
}

public record GetGRNListQuery : IRequest<IEnumerable<GRNSummaryDto>>
{
    public Guid CompanyId { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

