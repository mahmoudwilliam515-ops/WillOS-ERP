using EnterpriseERP.Application.Features.PurchaseReturns.Commands;
using EnterpriseERP.Application.Features.PurchaseReturns.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

/// <summary>
/// Purchase Returns Controller — مردودات المشتريات + إشعارات المدين
/// Blueprint Section P2P — Purchase Returns
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PurchaseReturnsController : ControllerBase
{
    private readonly ISender _sender;

    public PurchaseReturnsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>POST /api/v1/purchase-returns — إنشاء مردود مشتريات + Debit Note</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseReturnCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>GET /api/v1/purchase-returns/{id}</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken)
    {
        var query = new GetPurchaseReturnByIdQuery { Id = id, CompanyId = companyId };
        var result = await _sender.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>GET /api/v1/purchase-returns — قائمة مردودات المشتريات</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid companyId,
        [FromQuery] Guid? supplierId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetPurchaseReturnListQuery
        {
            CompanyId = companyId,
            SupplierId = supplierId,
            FromDate = fromDate,
            ToDate = toDate
        };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
