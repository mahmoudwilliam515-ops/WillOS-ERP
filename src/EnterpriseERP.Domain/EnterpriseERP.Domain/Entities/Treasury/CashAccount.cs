using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Accounting;

namespace EnterpriseERP.Domain.Entities.Treasury;

public class CashAccount : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public Guid LinkedAccountId { get; set; } // GL Account ID
    
    public string Currency { get; set; } = "EGP";
    
    // Navigation
    public Account LinkedAccount { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
