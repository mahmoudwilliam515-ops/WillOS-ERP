using EnterpriseERP.Application.Features.Taxation.Commands;
using EnterpriseERP.Application.Features.Taxation.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaxationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateTaxCategory([FromBody] CreateTaxCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetTaxCategories()
    {
        var query = new GetTaxCategoriesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("rules")]
    public async Task<IActionResult> CreateTaxRule([FromBody] CreateTaxRuleCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetTaxRules()
    {
        var query = new GetTaxRulesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
