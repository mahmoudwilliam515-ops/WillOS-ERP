using EnterpriseERP.Application.Features.Treasury.Commands;
using EnterpriseERP.Application.Features.Treasury.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

/// <summary>
/// Treasury Controller — الخزينة
/// Blueprint Section 1.4 — إيصالات + مدفوعات + بروبوزال + بنوك
/// </summary>
[ApiController]
[Route("api/v1/treasury")]
[Authorize]
public class TreasuryController : ControllerBase
{
    private readonly ISender _sender;

    public TreasuryController(ISender sender)
    {
        _sender = sender;
    }

    // ── Receipt Vouchers ────────────────────────────────────────────────────

    /// <summary>POST /api/v1/treasury/receipt-vouchers — إنشاء إيصال قبض</summary>
    [HttpPost("receipt-vouchers")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReceipt(
        [FromBody] CreateReceiptVoucherCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetReceiptById), new { id }, id);
    }

    [HttpGet("receipt-vouchers/{id:guid}")]
    public async Task<IActionResult> GetReceiptById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetReceiptVoucherByIdQuery { Id = id, CompanyId = companyId }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // ── Payment Vouchers ────────────────────────────────────────────────────

    /// <summary>POST /api/v1/treasury/payment-vouchers — إنشاء إيصال دفع</summary>
    [HttpPost("payment-vouchers")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentVoucherCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPaymentById), new { id }, id);
    }

    [HttpGet("payment-vouchers/{id:guid}")]
    public async Task<IActionResult> GetPaymentById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPaymentVoucherByIdQuery { Id = id, CompanyId = companyId }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // ── Payment Proposals ───────────────────────────────────────────────────

    /// <summary>POST /api/v1/treasury/payment-proposals — إنشاء اقتراح دفع دفعي</summary>
    [HttpPost("payment-proposals")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProposal(
        [FromBody] CreatePaymentProposalCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProposalById), new { id }, id);
    }

    [HttpGet("payment-proposals/{id:guid}")]
    public async Task<IActionResult> GetProposalById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPaymentProposalByIdQuery { Id = id, CompanyId = companyId }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>PATCH /api/v1/treasury/payment-proposals/{id}/approve — موافقة CFO</summary>
    [HttpPatch("payment-proposals/{id:guid}/approve")]
    [Authorize(Roles = "CFO,FinanceDirector")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApproveProposal(
        Guid id,
        [FromBody] ApprovePaymentProposalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApprovePaymentProposalCommand
        {
            ProposalId = id,
            CompanyId = request.CompanyId,
            ApprovedByUserId = request.ApprovedByUserId
        };
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>GET /api/v1/treasury/payment-proposals/{id}/export-iso20022 — تصدير XML للبنك</summary>
    [HttpGet("payment-proposals/{id:guid}/export-iso20022")]
    [Authorize(Roles = "CFO,FinanceDirector,Treasurer")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportISO20022(
        Guid id,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken)
    {
        var command = new ExportISO20022Command { ProposalId = id, CompanyId = companyId };
        var xmlContent = await _sender.Send(command, cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(xmlContent), "application/xml", $"pain001_{id:N}.xml");
    }

    // ── Bank Statement Import ───────────────────────────────────────────────

    /// <summary>POST /api/v1/treasury/bank-statements/import — استيراد كشف حساب بنكي</summary>
    [HttpPost("bank-statements/import")]
    [ProducesResponseType(typeof(BankStatementImportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportBankStatement(
        IFormFile file,
        [FromQuery] Guid companyId,
        [FromQuery] Guid bankAccountId,
        [FromQuery] string format, // MT940 | CAMT053
        CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var content = System.Text.Encoding.UTF8.GetString(ms.ToArray());

        var command = new ImportBankStatementCommand
        {
            CompanyId = companyId,
            BankAccountId = bankAccountId,
            FileContent = content,
            Format = format
        };
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── Cash Position ───────────────────────────────────────────────────────

    /// <summary>GET /api/v1/treasury/cash-position — لوحة تحكم الخزينة</summary>
    [HttpGet("cash-position")]
    [ProducesResponseType(typeof(CashPositionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCashPosition(
        [FromQuery] Guid companyId,
        [FromQuery] DateTime? asOfDate,
        CancellationToken cancellationToken)
    {
        var query = new GetCashPositionQuery
        {
            CompanyId = companyId,
            AsOfDate = asOfDate ?? DateTime.UtcNow.Date
        };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}

// ── Supplementary DTOs & Commands ─────────────────────────────────────────────

public record ApprovePaymentProposalRequest
{
    public Guid CompanyId { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

public record ApprovePaymentProposalCommand : IRequest<Unit>
{
    public Guid ProposalId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

public record ExportISO20022Command : IRequest<string>
{
    public Guid ProposalId { get; init; }
    public Guid CompanyId { get; init; }
}

public record GetReceiptVoucherByIdQuery : IRequest<object?>
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
}

public record GetPaymentVoucherByIdQuery : IRequest<object?>
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
}

public record GetPaymentProposalByIdQuery : IRequest<object?>
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
}
