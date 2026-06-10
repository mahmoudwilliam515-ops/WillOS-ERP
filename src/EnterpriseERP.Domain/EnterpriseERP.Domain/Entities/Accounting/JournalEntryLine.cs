using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Common;

namespace EnterpriseERP.Domain.Entities.Accounting;

public class JournalEntryLine : AuditableEntity, ICompanyEntity
{
    public Guid JournalEntryId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    
    // Amount in Transaction Currency
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    
    // Aliases for compatibility
    public decimal Debit => DebitAmount;
    public decimal Credit => CreditAmount;
    
    // Amount in Base Currency (Local Currency) - For IFRS Compliance
    public decimal BaseDebitAmount { get; set; }
    public decimal BaseCreditAmount { get; set; }
    
    public Guid CompanyId { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }
    
    // Sub-ledger tracking (Customer, Supplier, Employee, etc.)
    public Guid? PartyId { get; set; }
    
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

    // Navigation
    public JournalEntry JournalEntry { get; set; } = null!;
}
