using EnterpriseERP.Application.Features.Items.Commands.CreateItem;
using EnterpriseERP.Application.Features.Items.Queries.GetAllItems;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.ItemsView)]
    public async Task<IActionResult> GetAll()
    {
        var items = await _mediator.Send(new GetAllItemsQuery());

        return Ok(new
        {
            Success = true,
            Message = "Items retrieved successfully",
            Data = items,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.ItemsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateItemCommand command)
    {
        var item = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetAll), new { id = item.Id }, new
        {
            Success = true,
            Message = "Item created successfully",
            Data = item,
            Errors = Array.Empty<string>()
        });
    }
}
