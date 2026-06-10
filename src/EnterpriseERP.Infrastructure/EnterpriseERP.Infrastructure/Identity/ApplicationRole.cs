using Microsoft.AspNetCore.Identity;

namespace EnterpriseERP.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string Description { get; set; } = string.Empty;
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}
