using EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Commands.ApprovePaymentVoucher;
using EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Commands.CreatePaymentVoucher;
using EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Queries.GetAllPaymentVouchers;
using EnterpriseERP.Application.Features.Treasury.ReceiptVouchers.Commands.ApproveReceiptVoucher;
using EnterpriseERP.Application.Features.Treasury.ReceiptVouchers.Commands.CreateReceiptVoucher;
using EnterpriseERP.Application.Features.Treasury.ReceiptVouchers.Queries.GetAllReceiptVouchers;
using EnterpriseERP.Application.Features.Treasury.PaymentProposals.Queries.GetPaymentProposals;
using EnterpriseERP.Application.Features.Treasury.PaymentProposals.Commands.ProcessPaymentProposal;
using EnterpriseERP.Application.Features.Treasury.PaymentProposals.Commands.GenerateBankPaymentFile;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TreasuryController : ControllerBase
{
    private readonly IMediator _mediator;

    public TreasuryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("receipt-vouchers")]
    [HasPermission(Permissions.TreasuryView)]
    public async Task<IActionResult> GetAllReceiptVouchers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllReceiptVouchersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });

        return Ok(new
        {
            success = true,
            message = "Receipt vouchers retrieved successfully",
            data = result
        });
    }

    [HttpGet("payment-vouchers")]
    [HasPermission(Permissions.TreasuryView)]
    public async Task<IActionResult> GetAllPaymentVouchers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllPaymentVouchersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });

        return Ok(new
        {
            success = true,
            message = "Payment vouchers retrieved successfully",
            data = result
        });
    }

    [HttpPost("receipt-vouchers")]
    [HasPermission(Permissions.TreasuryManage)]
    public async Task<IActionResult> CreateReceiptVoucher([FromBody] CreateReceiptVoucherCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("payment-vouchers")]
    [HasPermission(Permissions.TreasuryManage)]
    public async Task<IActionResult> CreatePaymentVoucher([FromBody] CreatePaymentVoucherCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("receipt-vouchers/{id:guid}/approve")]
    [HasPermission(Permissions.TreasuryManage)]
    public async Task<IActionResult> ApproveReceiptVoucher(Guid id)
    {
        var result = await _mediator.Send(new ApproveReceiptVoucherCommand(id));
        return Ok(new { success = result, message = result ? "Receipt voucher approved and AR updated." : "Voucher not found or already approved." });
    }

    [HttpPost("payment-vouchers/{id:guid}/approve")]
    [HasPermission(Permissions.TreasuryManage)]
    public async Task<IActionResult> ApprovePaymentVoucher(Guid id)
    {
        var result = await _mediator.Send(new ApprovePaymentVoucherCommand(id));
        return Ok(new { success = result, message = result ? "Payment voucher approved and AP updated." : "Voucher not found or already approved." });
    }

    [HttpGet("payment-proposals")]
    [HasPermission(Permissions.TreasuryView)]
    public async Task<IActionResult> GetPaymentProposals([FromQuery] decimal? budget, [FromQuery] Guid? supplierId)
    {
        var result = await _mediator.Send(new GetPaymentProposalsQuery { MaxTotalBudget = budget, SupplierId = supplierId });
        return Ok(new { success = true, data = result });
    }

    [HttpPost("payment-proposals/process")]
    [HasPermission(Permissions.TreasuryManage)]
    public async Task<IActionResult> ProcessPaymentProposal([FromBody] ProcessPaymentProposalCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("bank-payment-file")]
    [HasPermission(Permissions.TreasuryManage)]
    public async Task<IActionResult> GenerateBankPaymentFile([FromBody] GenerateBankPaymentFileCommand command)
    {
        var xmlContent = await _mediator.Send(command);
        var fileName = $"ISO20022_Payment_{DateTime.UtcNow:yyyyMMddHHmmss}.xml";
        return File(System.Text.Encoding.UTF8.GetBytes(xmlContent), "application/xml", fileName);
    }
}
