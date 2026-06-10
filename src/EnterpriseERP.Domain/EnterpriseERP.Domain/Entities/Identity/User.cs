using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Identity;

public class User : AuditableEntity, ISoftDelete
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLogin { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
