using EnterpriseERP.Application.Features.Customers.Commands.CreateCustomer;
using EnterpriseERP.Application.Features.Customers.Queries.GetAllCustomers;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.CustomersView)]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _mediator.Send(new GetAllCustomersQuery());

        return Ok(new
        {
            Success = true,
            Message = "Customers retrieved successfully",
            Data = customers,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.CustomersCreate)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var customer = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetAll), new { id = customer.Id }, new
        {
            Success = true,
            Message = "Customer created successfully",
            Data = customer,
            Errors = Array.Empty<string>()
        });
    }
}
