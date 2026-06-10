using System.Security.Claims;
using EnterpriseERP.Application.Common.Interfaces.Services;

namespace EnterpriseERP.API.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
        }
    }

    public Guid? CompanyId
    {
        get
        {
            var companyIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue("CompanyId") 
                               ?? httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
            return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
        }
    }

    public string? FullName => httpContextAccessor.HttpContext?.User?.FindFirstValue("FullName");

    public Guid? BranchId
    {
        get
        {
            var branchIdClaim = httpContextAccessor.HttpContext?.User?.FindFirstValue("BranchId");
            return Guid.TryParse(branchIdClaim, out var branchId) ? branchId : null;
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
