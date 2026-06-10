using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Accounting;

public class GeneralLedgerEntry : AuditableEntity, ICompanyEntity
{
    public Guid CompanyId { get; set; }
    public DateTime PostingDate { get; set; }
    
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public AccountCategory AccountCategory { get; set; }
    
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceModule { get; set; } = string.Empty;
    public Guid? SourceEntityId { get; set; }
}
