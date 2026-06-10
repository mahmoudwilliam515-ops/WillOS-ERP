using EnterpriseERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EnterpriseERP.Infrastructure.Identity.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationDbContext _dbContext;

    public PermissionAuthorizationHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null || !context.User.Identity!.IsAuthenticated)
        {
            return;
        }

        // Fast path: check if the permission is directly in claims (used in testing or future claim-based tokens)
        var hasClaimPermission = context.User.Claims
            .Any(c => c.Type == "permission" && c.Value == requirement.Permission);

        if (hasClaimPermission)
        {
            context.Succeed(requirement);
            return;
        }

        // DB path: check role-based permissions
        var userId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var userRoles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (!userRoles.Any())
        {
            return;
        }

        var hasPermission = await _dbContext.RolePermissions
            .AnyAsync(rp => userRoles.Contains(rp.RoleId) && rp.Permission == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
