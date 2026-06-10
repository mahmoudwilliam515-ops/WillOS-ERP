using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers.v1;

/// <summary>
/// Phase 9 §9.7.1 (A9) — API Inventory Management.
/// Returns spec version history and deprecation notices so clients can
/// track endpoint lifecycle per the API Governance spec.
/// </summary>
[ApiController]
[Route("api/v1/changelog")]
public class ApiChangelogController : ControllerBase
{
    private static readonly IReadOnlyList<ApiVersionEntry> _changelog = new[]
    {
        new ApiVersionEntry(
            Version:     "v1.0",
            ReleasedAt:  new DateOnly(2026, 6, 5),
            Status:      "STABLE",
            Summary:     "Initial release — AP Invoices, Workflow Engine, API Governance layer",
            Endpoints:   new[]
            {
                "POST   /api/v1/ap/invoices",
                "GET    /api/v1/ap/invoices",
                "PATCH  /api/v1/ap/invoices/{id}",
                "DELETE /api/v1/ap/invoices/{id}",
                "POST   /api/v1/workflows/submit",
                "POST   /api/v1/workflows/{id}/approve",
                "POST   /api/v1/workflows/{id}/reject",
            },
            DeprecatedAt: null,
            SunsetAt:    null
        )
    };

    /// <summary>
    /// Returns the full API version changelog.
    /// Includes stable, deprecated, and sunset version records.
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 3600)] // Cache for 1 hour — changes rarely
    public IActionResult GetChangelog()
        => Ok(new { changelog = _changelog });
}

/// <param name="Version">Semver API version string (e.g. "v1.0")</param>
/// <param name="ReleasedAt">Release date</param>
/// <param name="Status">STABLE | DEPRECATED | SUNSET</param>
/// <param name="Summary">Human-readable release summary</param>
/// <param name="Endpoints">List of endpoints introduced in this version</param>
/// <param name="DeprecatedAt">Date this version was deprecated (null if still stable)</param>
/// <param name="SunsetAt">Date this version will be removed (null if no sunset scheduled)</param>
public record ApiVersionEntry(
    string        Version,
    DateOnly      ReleasedAt,
    string        Status,
    string        Summary,
    string[]      Endpoints,
    DateOnly?     DeprecatedAt,
    DateOnly?     SunsetAt
);
