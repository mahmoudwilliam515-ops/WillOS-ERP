using EnterpriseERP.Application.Features.Lookups.Queries.GetLookups;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission(Permissions.LookupsView)]
public class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public LookupsController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("accounting-periods")]
    public async Task<IActionResult> GetAccountingPeriods()
    {
        var periods = await _unitOfWork.Repository<EnterpriseERP.Domain.Entities.Accounting.AccountingPeriod>()
            .Query()
            .OrderByDescending(p => p.StartDate)
            .Select(p => new
            {
                p.Id,
                periodName = p.PeriodName,
                p.Status,
                p.StartDate,
                p.EndDate,
                p.IsInventoryReconciled,
                p.IsARReconciled,
                p.IsAPReconciled,
                p.IsFixedAssetsDepreciated,
                p.IsPayrollPosted,
                p.IsBankReconciled
            })
            .ToListAsync();
        return Ok(new { success = true, data = periods });
    }

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetCustomersLookupQuery(searchTerm));
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers([FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetSuppliersLookupQuery(searchTerm));
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses([FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetWarehousesLookupQuery(searchTerm));
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("items")]
    public async Task<IActionResult> GetItems([FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetItemsLookupQuery(searchTerm));
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches([FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetBranchesLookupQuery(searchTerm));
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("fixed-assets")]
    public async Task<IActionResult> GetFixedAssets([FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetFixedAssetsLookupQuery(searchTerm));
        return Ok(new { Success = true, Data = result });
    }

    [HttpGet("technicians")]
    public async Task<IActionResult> GetTechnicians([FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetTechniciansLookupQuery(searchTerm));
        return Ok(new { Success = true, Data = result });
    }
}
