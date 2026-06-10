using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Migration.Commands.ImportOpeningBalances;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Domain.Entities.Migration;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OpeningBalancesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public OpeningBalancesController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("sessions")]
    [HasPermission(Permissions.OpeningBalancesView)]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await _unitOfWork.Repository<OpeningBalanceSession>().GetAllAsync();
        var ordered = sessions.OrderByDescending(s => s.CreatedAt).Select(s => new
        {
            s.Id,
            s.SessionName,
            s.AsOfDate,
            Status = s.Status.ToString(),
            s.CustomerCount,
            s.SupplierCount,
            s.InventoryItemCount,
            s.TotalARBalance,
            s.TotalAPBalance,
            s.TotalInventoryValue,
            s.JournalEntryId,
            s.Notes,
            s.CreatedAt,
            s.ErrorMessage
        });
        return Ok(new { success = true, data = ordered });
    }

    [HttpPost("import")]
    [HasPermission(Permissions.OpeningBalancesImport)]
    public async Task<IActionResult> Import([FromBody] ImportOpeningBalancesCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new
        {
            success = true,
            message = $"Opening balances imported successfully. Journal Entry: {result.JournalEntryNumber}",
            data = result
        });
    }
}
