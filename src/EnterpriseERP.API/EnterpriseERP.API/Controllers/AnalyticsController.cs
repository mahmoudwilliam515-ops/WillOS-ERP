using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.Reports.Queries.GetGlobalAnalytics;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAIPredictiveService _predictiveService;

    public AnalyticsController(IMediator mediator, IAIPredictiveService predictiveService)
    {
        _mediator = mediator;
        _predictiveService = predictiveService;
    }

    [HttpGet("predict-cashflow")]
    [HasPermission(Permissions.FinancialReportsView)]
    public async Task<IActionResult> PredictCashFlow([FromQuery] int days = 30)
    {
        var result = await _predictiveService.PredictCashFlowAsync(days);
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("predict-inventory")]
    [HasPermission(Permissions.FinancialReportsView)]
    public async Task<IActionResult> PredictInventory()
    {
        var result = await _predictiveService.PredictInventoryReplenishmentAsync();
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("global")]
    [HasPermission(Permissions.FinancialReportsView)]
    public async Task<IActionResult> GetGlobalAnalytics([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        if (fromDate == default) fromDate = DateTime.UtcNow.AddMonths(-6);
        if (toDate == default) toDate = DateTime.UtcNow;

        var query = new GetGlobalAnalyticsQuery { FromDate = fromDate, ToDate = toDate };
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }
}
