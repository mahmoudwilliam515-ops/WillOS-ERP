using EnterpriseERP.Application.Features.Reports.BalanceSheet;
using EnterpriseERP.Application.Features.Reports.CashFlow;
using EnterpriseERP.Application.Features.PeriodClose.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

/// <summary>
/// Reports Controller — القوائم المالية
/// Blueprint Section 1.3 — R2R
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    // ── Balance Sheet ────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/reports/balance-sheet — الميزانية العمومية</summary>
    [HttpGet("balance-sheet")]
    [ProducesResponseType(typeof(BalanceSheetDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceSheet(
        [FromQuery] Guid companyId,
        [FromQuery] Guid periodId,
        CancellationToken cancellationToken)
    {
        var query = new GetBalanceSheetQuery { CompanyId = companyId, PeriodId = periodId };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    // ── Cash Flow Statement ─────────────────────────────────────────────────

    /// <summary>GET /api/v1/reports/cash-flow-statement — قائمة التدفقات النقدية</summary>
    [HttpGet("cash-flow-statement")]
    [ProducesResponseType(typeof(CashFlowStatementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCashFlowStatement(
        [FromQuery] Guid companyId,
        [FromQuery] Guid periodId,
        CancellationToken cancellationToken)
    {
        var query = new GetCashFlowQuery { CompanyId = companyId, PeriodId = periodId };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    // ── Trial Balance ────────────────────────────────────────────────────────

    /// <summary>GET /api/v1/reports/trial-balance — ميزان المراجعة</summary>
    [HttpGet("trial-balance")]
    [ProducesResponseType(typeof(TrialBalanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] Guid companyId,
        [FromQuery] Guid periodId,
        CancellationToken cancellationToken)
    {
        var query = new GetTrialBalanceQuery { CompanyId = companyId, PeriodId = periodId };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    // ── Subledger Reconciliation ─────────────────────────────────────────────

    /// <summary>POST /api/v1/reports/reconcile-subledger — مطابقة Subledger مع GL</summary>
    [HttpPost("reconcile-subledger")]
    [Authorize(Roles = "CFO,FinanceDirector,Accountant")]
    [ProducesResponseType(typeof(ReconciliationResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReconcileSubledger(
        [FromBody] ReconcileSubledgerToGLCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    // ── Period Close ─────────────────────────────────────────────────────────

    /// <summary>POST /api/v1/reports/period-close/initiate — بدء عملية إغلاق الفترة</summary>
    [HttpPost("period-close/initiate")]
    [Authorize(Roles = "CFO,FinanceDirector")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> InitiatePeriodClose(
        [FromBody] InitiatePeriodCloseCommand command,
        CancellationToken cancellationToken)
    {
        var checklistId = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPeriodCloseStatus), new { id = checklistId }, checklistId);
    }

    /// <summary>GET /api/v1/reports/period-close/{id} — حالة إغلاق الفترة</summary>
    [HttpGet("period-close/{id:guid}")]
    [ProducesResponseType(typeof(PeriodCloseStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriodCloseStatus(
        Guid id,
        [FromQuery] Guid companyId,
        CancellationToken cancellationToken)
    {
        var query = new GetPeriodCloseStatusQuery { ChecklistId = id, CompanyId = companyId };
        var result = await _sender.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>PATCH /api/v1/reports/period-close/{id}/approve — موافقة CFO على الإغلاق</summary>
    [HttpPatch("period-close/{id:guid}/approve")]
    [Authorize(Roles = "CFO")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApprovePeriodClose(
        Guid id,
        [FromBody] ApprovePeriodCloseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApprovePeriodCloseCommand
        {
            ChecklistId = id,
            CompanyId = request.CompanyId,
            ApprovedByUserId = request.ApprovedByUserId
        };
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    // ── AR Aging Report ─────────────────────────────────────────────────────

    /// <summary>GET /api/v1/reports/ar-aging — تقرير تقادم الذمم المدينة</summary>
    [HttpGet("ar-aging")]
    [ProducesResponseType(typeof(ARAgingReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetARAgingReport(
        [FromQuery] Guid companyId,
        [FromQuery] DateTime? asOfDate,
        CancellationToken cancellationToken)
    {
        var query = new GetARAgingReportQuery
        {
            CompanyId = companyId,
            AsOfDate = asOfDate ?? DateTime.UtcNow.Date
        };
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}

// ── DTOs & Supplementary Commands ─────────────────────────────────────────────

public record ApprovePeriodCloseRequest
{
    public Guid CompanyId { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

public record ApprovePeriodCloseCommand : IRequest<Unit>
{
    public Guid ChecklistId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

public record GetPeriodCloseStatusQuery : IRequest<PeriodCloseStatusDto?>
{
    public Guid ChecklistId { get; init; }
    public Guid CompanyId { get; init; }
}

public record PeriodCloseStatusDto
{
    public Guid Id { get; init; }
    public string PeriodName { get; init; } = default!;
    public string Status { get; init; } = default!;
    public List<PeriodCloseStepDto> Steps { get; init; } = new();
}

public record PeriodCloseStepDto
{
    public string StepName { get; init; } = default!;
    public bool IsCompleted { get; init; }
    public string? CompletedByRole { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record TrialBalanceDto
{
    public Guid CompanyId { get; init; }
    public string PeriodName { get; init; } = default!;
    public List<TrialBalanceLineDto> Lines { get; init; } = new();
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public bool IsBalanced => TotalDebits == TotalCredits;
}

public record TrialBalanceLineDto
{
    public string AccountCode { get; init; } = default!;
    public string AccountName { get; init; } = default!;
    public decimal DebitBalance { get; init; }
    public decimal CreditBalance { get; init; }
}

public record GetTrialBalanceQuery : IRequest<TrialBalanceDto>
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
}

public record ARAgingReportDto
{
    public Guid CompanyId { get; init; }
    public DateTime AsOfDate { get; init; }
    public List<ARAgingCustomerDto> Customers { get; init; } = new();
    public decimal TotalCurrent { get; init; }
    public decimal Total1To30 { get; init; }
    public decimal Total31To60 { get; init; }
    public decimal Total61To90 { get; init; }
    public decimal TotalOver90 { get; init; }
    public decimal GrandTotal { get; init; }
}

public record ARAgingCustomerDto
{
    public string CustomerName { get; init; } = default!;
    public decimal Current { get; init; }
    public decimal Days1To30 { get; init; }
    public decimal Days31To60 { get; init; }
    public decimal Days61To90 { get; init; }
    public decimal Over90 { get; init; }
    public decimal Total { get; init; }
}

public record GetARAgingReportQuery : IRequest<ARAgingReportDto>
{
    public Guid CompanyId { get; init; }
    public DateTime AsOfDate { get; init; }
}

public record ReconciliationResultDto
{
    public bool IsReconciled { get; init; }
    public decimal ARSubledgerTotal { get; init; }
    public decimal APSubledgerTotal { get; init; }
    public decimal ARGLControlTotal { get; init; }
    public decimal APGLControlTotal { get; init; }
    public decimal ARVariance { get; init; }
    public decimal APVariance { get; init; }
    public List<string> Discrepancies { get; init; } = new();
}
