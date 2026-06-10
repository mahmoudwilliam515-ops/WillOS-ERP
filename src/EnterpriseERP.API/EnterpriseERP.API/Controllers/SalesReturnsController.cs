using EnterpriseERP.Application.Features.SalesReturns.Commands;
using EnterpriseERP.Application.Features.SalesReturns.Queries;
using EnterpriseERP.Application.Features.Dunning.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

/// <summary>
/// Sales Returns Controller â€” Ù…Ø±Ø¯ÙˆØ¯Ø§Øª Ø§Ù„Ù…Ø¨ÙŠØ¹Ø§Øª + Ø¥Ø´Ø¹Ø§Ø±Ø§Øª Ø§Ù„Ø¯Ø§Ø¦Ù†
/// Blueprint Section O2C-09
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SalesReturnsController : ControllerBase
{
    private readonly ISender _sender;

    public SalesReturnsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>POST /api/v1/sales-returns â€” Ø¥Ù†Ø´Ø§Ø¡ Ù…Ø±Ø¯ÙˆØ¯ Ù…Ø¨ÙŠØ¹Ø§Øª + Credit Note</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCreditNoteCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>GET /api/v1/sales-returns/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken)
    {
        var query = new GetSalesReturnByIdQuery { Id = id, CompanyId = companyId };
        var result = await _sender.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>GET /api/v1/sales-returns â€” Ù‚Ø§Ø¦Ù…Ø© Ù…Ø±Ø¯ÙˆØ¯Ø§Øª Ø§Ù„Ù…Ø¨ÙŠØ¹Ø§Øª</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? customerId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetSalesReturnListQuery
        {
            CompanyId = companyId,
            CustomerId = customerId,
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}

