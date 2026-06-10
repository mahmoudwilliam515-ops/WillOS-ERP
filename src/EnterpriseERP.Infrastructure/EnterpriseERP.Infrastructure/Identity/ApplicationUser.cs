using Microsoft.AspNetCore.Identity;

namespace EnterpriseERP.Infrastructure.Identity;

/// <summary>
/// Application User extending ASP.NET Identity IdentityUser.
/// Adds ERP-specific fields for multi-branch and audit support.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}
