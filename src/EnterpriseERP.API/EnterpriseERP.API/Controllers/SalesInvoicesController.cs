using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.SalesInvoices.Commands.ApproveSalesInvoice;
using EnterpriseERP.Application.Features.SalesInvoices.Commands.SubmitEInvoice;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using EnterpriseERP.Application.Features.SalesInvoices.Queries.GetAllSalesInvoices;
using EnterpriseERP.Application.Features.SalesInvoices.Queries.GetEInvoicePreview;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesInvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public SalesInvoicesController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [HasPermission(Permissions.SalesInvoicesView)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] string? searchTerm = null)
    {
        var invoices = await _mediator.Send(new GetAllSalesInvoicesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });

        return Ok(new
        {
            Success = true,
            Message = "Sales invoices retrieved successfully",
            Data = invoices,
            Errors = Array.Empty<string>()
        });
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.SalesInvoicesView)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var invoice = await _mediator.Send(new EnterpriseERP.Application.Features.SalesInvoices.Queries.GetSalesInvoiceById.GetSalesInvoiceByIdQuery(id));

        return Ok(new
        {
            Success = true,
            Message = "Sales invoice retrieved successfully",
            Data = invoice,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.SalesInvoicesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceCommand command)
    {
        var invoice = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetAll), new { id = invoice.Id }, new
        {
            Success = true,
            Message = "Sales invoice created successfully as Draft",
            Data = invoice,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost("{id}/approve")]
    [HasPermission(Permissions.SalesInvoicesApprove)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveSalesInvoiceCommand(
            id, 
            _currentUserService.UserId ?? "System", 
            _currentUserService.TenantId ?? Guid.Empty));

        return Ok(new
        {
            Success = true,
            Message = "Sales invoice approved and posted successfully",
            Data = result,
            Errors = Array.Empty<string>()
        });
    }
    [HttpGet("{id}/e-invoice/preview")]
    [HasPermission(Permissions.SalesInvoicesView)]
    public async Task<IActionResult> GetEInvoicePreview(Guid id)
    {
        var preview = await _mediator.Send(new GetEInvoicePreviewQuery(id));
        return Ok(new
        {
            Success = true,
            Message = "E-invoice preview loaded",
            Data = preview,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost("{id}/e-invoice/submit")]
    [HasPermission(Permissions.SalesInvoicesApprove)]
    public async Task<IActionResult> SubmitEInvoice(Guid id)
    {
        var result = await _mediator.Send(new SubmitEInvoiceCommand(id));
        return Ok(new
        {
            Success = result.Success,
            Message = result.Message,
            Data = result,
            Errors = result.Success ? Array.Empty<string>() : new[] { result.Message }
        });
    }

    [HttpPost("{id}/reverse")]
    [HasPermission(Permissions.SalesInvoicesApprove)]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] ReverseInvoiceRequest request)
    {
        var success = await _mediator.Send(new EnterpriseERP.Application.Features.SalesInvoices.Commands.ReverseSalesInvoice.ReverseSalesInvoiceCommand(id, request.Reason));

        return Ok(new
        {
            Success = success,
            Message = success ? "Sales invoice reversed successfully" : "Failed to reverse invoice",
            Data = success,
            Errors = Array.Empty<string>()
        });
    }
}

public class ReverseInvoiceRequest
{
    public string Reason { get; set; } = string.Empty;
}
