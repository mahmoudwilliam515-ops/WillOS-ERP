using EnterpriseERP.Application.Features.Reports.Queries;
using EnterpriseERP.Application.Common.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/v1/reporting")]
public class ReportingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ReportingController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Financial Statement Builder API (Phase 6)
    /// </summary>
    [HttpGet("financial-statements/{type}")]
    public async Task<IActionResult> GetFinancialStatement(
        string type,
        [FromQuery] Guid periodId,
        [FromQuery] Guid? comparativePeriodId,
        [FromQuery] string gaapBook = "IFRS")
    {
        var companyId = _currentUserService.CompanyId;
        if (companyId == Guid.Empty)
            return BadRequest(new { Success = false, Message = "Company context is missing." });

        var query = new GetFinancialStatementQuery(companyId.Value, periodId, comparativePeriodId, gaapBook, type.ToUpper());
        try
        {
            var result = await _mediator.Send(query);
            return Ok(new { Success = true, Data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}
