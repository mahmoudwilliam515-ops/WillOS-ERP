using Microsoft.AspNetCore.Identity;

namespace EnterpriseERP.Infrastructure.Identity;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public ApplicationUser User { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}
