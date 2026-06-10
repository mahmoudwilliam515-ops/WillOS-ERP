using EnterpriseERP.Application.Features.Companies.Commands.CreateCompany;
using EnterpriseERP.Application.Features.Companies.Queries.GetAllCompanies;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.CompaniesView)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var companies = await mediator.Send(new GetAllCompaniesQuery { ActiveOnly = activeOnly });
        return Ok(new
        {
            Success = true,
            Message = "Companies retrieved successfully",
            Data = companies,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.CompaniesManage)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyCommand command)
    {
        var company = await mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id = company.Id }, new
        {
            Success = true,
            Message = "Company created successfully",
            Data = company,
            Errors = Array.Empty<string>()
        });
    }
}
