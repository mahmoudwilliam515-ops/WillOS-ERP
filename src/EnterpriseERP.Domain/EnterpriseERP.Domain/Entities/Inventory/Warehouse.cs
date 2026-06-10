using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Inventory;

public class Warehouse : AuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsMain { get; set; } = false;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
