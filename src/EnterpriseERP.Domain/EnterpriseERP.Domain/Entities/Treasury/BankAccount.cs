using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Accounting;

namespace EnterpriseERP.Domain.Entities.Treasury;

public class BankAccount : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public Guid LinkedAccountId { get; set; } // GL Account ID
    
    public string Currency { get; set; } = "EGP";
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;

    public void Debit(decimal amount)
    {
        CurrentBalance -= amount;
    }

    public void Credit(decimal amount)
    {
        CurrentBalance += amount;
    }

    // Navigation
    public Account LinkedAccount { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
