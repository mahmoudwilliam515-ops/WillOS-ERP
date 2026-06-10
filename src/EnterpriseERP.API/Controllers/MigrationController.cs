using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission(Permissions.SystemMigrate)]
public class MigrationController : ControllerBase
{
    private readonly ILegacyDataMigrationService _migrationService;

    public MigrationController(ILegacyDataMigrationService migrationService)
    {
        _migrationService = migrationService;
    }

    [HttpPost("migrate-legacy")]
    public async Task<IActionResult> MigrateLegacyData([FromBody] MigrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LegacyConnectionString))
        {
            return BadRequest(new { Success = false, Message = "Legacy connection string is required." });
        }

        var result = await _migrationService.MigrateLegacyDataAsync(request.LegacyConnectionString);

        return Ok(new { Success = true, Message = "Migration execution completed.", Details = result });
    }
}

public class MigrationRequest
{
    public string LegacyConnectionString { get; set; } = string.Empty;
}
