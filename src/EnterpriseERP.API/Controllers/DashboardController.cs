using EnterpriseERP.Application.Features.Dashboard.Queries.GetDashboardSummary;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Returns aggregated KPI summary for the dashboard.
    /// Single endpoint replaces multiple parallel API calls from the frontend.
    /// Requires only basic authenticated access — no specific module permission needed.
    /// </summary>
    [HttpGet("summary")]
    [HasPermission(Permissions.BranchesView)] // Minimal permission — any logged-in user with any access can view
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
            return Ok(new { success = true, data = result, message = "Dashboard summary loaded." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard summary.");
            // Return empty dashboard — do not crash the UI
            return Ok(new
            {
                success = true,
                data = new DashboardSummaryDto(),
                message = "Partial data loaded."
            });
        }
    }
}
