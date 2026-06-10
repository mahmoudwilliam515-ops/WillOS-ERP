using EnterpriseERP.Application.Features.Reports.Queries;
using EnterpriseERP.Application.Features.Reports.Queries.GetSubledgerReconciliation;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission(Permissions.FinancialReportsView)]
public class FinancialReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinancialReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Subledger to GL Reconciliation Report
    /// </summary>
    [HttpGet("subledger-reconciliation")]
    public async Task<IActionResult> GetSubledgerReconciliation([FromQuery] DateTime asOfDate)
    {
        if (asOfDate == default) asOfDate = DateTime.UtcNow;
        var result = await _mediator.Send(new GetSubledgerReconciliationQuery(asOfDate));
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// Profit & Loss Statement for a date range
    /// </summary>
    [HttpGet("profit-and-loss")]
    public async Task<IActionResult> GetProfitAndLoss(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        if (fromDate > toDate)
            return BadRequest(new { Success = false, Message = "fromDate must be before toDate." });

        var result = await _mediator.Send(new GetProfitAndLossQuery(fromDate, toDate));
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// Trial Balance as of a specific date
    /// </summary>
    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime asOfDate)
    {
        var result = await _mediator.Send(new GetTrialBalanceQuery(asOfDate));
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// Customer Aging Report
    /// </summary>
    [HttpGet("customer-aging")]
    public async Task<IActionResult> GetCustomerAging([FromQuery] EnterpriseERP.Application.Features.Reports.Queries.GetCustomerAgingReport.GetCustomerAgingReportQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// Supplier Aging Report
    /// </summary>
    [HttpGet("supplier-aging")]
    public async Task<IActionResult> GetSupplierAging([FromQuery] EnterpriseERP.Application.Features.Reports.Queries.GetSupplierAgingReport.GetSupplierAgingReportQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// Balance Sheet as of a specific date
    /// </summary>
    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime asOfDate)
    {
        var result = await _mediator.Send(new GetBalanceSheetQuery { AsOfDate = asOfDate });
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// Cash Flow Statement for a date range
    /// </summary>
    [HttpGet("cash-flow")]
    public async Task<IActionResult> GetCashFlow([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        if (fromDate > toDate)
            return BadRequest(new { Success = false, Message = "fromDate must be before toDate." });

        var result = await _mediator.Send(new GetCashFlowQuery { StartDate = fromDate, EndDate = toDate });
        return Ok(new { Success = true, Data = result });
    }
}
