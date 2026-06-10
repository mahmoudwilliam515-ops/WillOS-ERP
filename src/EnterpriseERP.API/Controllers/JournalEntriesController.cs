using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using EnterpriseERP.Application.Features.Accounting.JournalEntries.Queries.GetAllJournalEntries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public JournalEntriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.JournalEntriesView)]
    public async Task<IActionResult> GetAll()
    {
        var entries = await _mediator.Send(new GetAllJournalEntriesQuery());
        return Ok(new
        {
            Success = true,
            Message = "Journal entries retrieved successfully",
            Data = entries,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.JournalEntriesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryCommand command)
    {
        var entry = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = entry.Id }, new
        {
            Success = true,
            Message = "Journal entry created successfully",
            Data = entry,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost("{id:guid}/reverse")]
    [HasPermission(Permissions.JournalEntriesCreate)]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] EnterpriseERP.Application.Features.Accounting.JournalEntries.Commands.ReverseJournalEntry.ReverseJournalEntryCommand command)
    {
        if (id != command.OriginalJournalEntryId)
            return BadRequest(new { Success = false, Message = "Route ID must match Command ID." });

        try
        {
            var reversalEntry = await _mediator.Send(command);
            return Ok(new
            {
                Success = true,
                Message = "Journal entry reversed successfully",
                Data = reversalEntry,
                Errors = Array.Empty<string>()
            });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
}
