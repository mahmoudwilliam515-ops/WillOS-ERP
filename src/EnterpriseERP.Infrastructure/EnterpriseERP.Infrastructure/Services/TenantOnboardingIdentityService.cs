using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EnterpriseERP.Infrastructure.Services;

public class TenantOnboardingIdentityService : ITenantOnboardingIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public TenantOnboardingIdentityService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<(Guid UserId, string Email)> CreateAdminUserAndRolesAsync(Guid tenantId, string email, string fullName, string password)
    {
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = email,
            Email = email,
            FullName = fullName,
            TenantId = tenantId,
            EmailConfirmed = true
        };

        var createUserResult = await _userManager.CreateAsync(adminUser, password);
        if (!createUserResult.Succeeded)
        {
            throw new Exception($"Failed to create admin user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
        }

        var roles = new[]
        {
            new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN".ToUpper() },
            new ApplicationRole { Name = "Accountant", NormalizedName = "ACCOUNTANT".ToUpper() },
            new ApplicationRole { Name = "SalesManager", NormalizedName = "SALESMANAGER".ToUpper() },
            new ApplicationRole { Name = "PurchasingManager", NormalizedName = "PURCHASINGMANAGER".ToUpper() },
            new ApplicationRole { Name = "WarehouseManager", NormalizedName = "WAREHOUSEMANAGER".ToUpper() },
            new ApplicationRole { Name = "Viewer", NormalizedName = "VIEWER".ToUpper() }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name))
            {
                await _roleManager.CreateAsync(role);
            }
        }

        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole != null)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }

        return (Guid.Parse(adminUser.Id), adminUser.Email);
    }
}
