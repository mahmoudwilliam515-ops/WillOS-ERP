using EnterpriseERP.Application.Features.Suppliers.Commands.CreateSupplier;
using EnterpriseERP.Application.Features.Suppliers.Queries.GetAllSuppliers;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuppliersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.SuppliersView)]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _mediator.Send(new GetAllSuppliersQuery());

        return Ok(new
        {
            Success = true,
            Message = "Suppliers retrieved successfully",
            Data = suppliers,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.SuppliersCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierCommand command)
    {
        var supplier = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetAll), new { id = supplier.Id }, new
        {
            Success = true,
            Message = "Supplier created successfully",
            Data = supplier,
            Errors = Array.Empty<string>()
        });
    }
}
