using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BankAccountsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public BankAccountsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [HasPermission(Permissions.TreasuryView)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _unitOfWork.Repository<BankAccount>().FindAsync(x => true);
        return Ok(new { success = true, data = result });
    }
}
