using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Identity;

public class Role : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
