using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Common;

namespace EnterpriseERP.Domain.Entities.Settings;

public class Branch : AuditableEntity, ISoftDelete, ICompanyEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
