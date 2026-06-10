using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using EnterpriseERP.Application.Features.PurchaseInvoices.Queries.GetAllPurchaseInvoices;
using EnterpriseERP.Application.Features.PurchaseInvoices.Commands.MatchPurchaseInvoice;
using EnterpriseERP.Application.Features.PurchaseInvoices.Queries.GetThreeWayMatchPreview;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseInvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseInvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.PurchaseInvoicesView)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] string? searchTerm = null)
    {
        var invoices = await _mediator.Send(new GetAllPurchaseInvoicesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });
        return Ok(new
        {
            Success = true,
            Message = "Purchase invoices retrieved successfully",
            Data = invoices,
            Errors = Array.Empty<string>()
        });
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.PurchaseInvoicesView)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var invoice = await _mediator.Send(new EnterpriseERP.Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById.GetPurchaseInvoiceByIdQuery(id));
        return Ok(new
        {
            Success = true,
            Message = "Purchase invoice retrieved successfully",
            Data = invoice,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.PurchaseInvoicesCreate)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceCommand command)
    {
        var invoice = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = invoice.Id }, new
        {
            Success = true,
            Message = "Purchase invoice created successfully as Draft",
            Data = invoice,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost("{id}/approve")]
    [HasPermission(Permissions.PurchaseInvoicesApprove)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var success = await _mediator.Send(new EnterpriseERP.Application.Features.PurchaseInvoices.Commands.ApprovePurchaseInvoice.ApprovePurchaseInvoiceCommand(id));
        return Ok(new
        {
            Success = success,
            Message = success ? "Purchase invoice approved and posted successfully" : "Failed to approve invoice",
            Data = success,
            Errors = Array.Empty<string>()
        });
    }

    [HttpGet("{id}/three-way-match")]
    [HasPermission(Permissions.PurchaseInvoicesView)]
    public async Task<IActionResult> GetThreeWayMatchPreview(Guid id)
    {
        var preview = await _mediator.Send(new GetThreeWayMatchPreviewQuery(id));
        return Ok(new
        {
            Success = true,
            Message = "Three-way match preview loaded",
            Data = preview,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost("{id}/match")]
    [HasPermission(Permissions.PurchaseInvoicesApprove)]
    public async Task<IActionResult> Match(Guid id)
    {
        var result = await _mediator.Send(new MatchPurchaseInvoiceCommand(id));
        return Ok(new
        {
            Success = result.IsSuccess,
            Message = result.IsSuccess ? "Matching completed" : result.Error.Message,
            Data = result.IsSuccess,
            Errors = result.IsSuccess ? Array.Empty<string>() : new[] { result.Error.Message }
        });
    }
}
