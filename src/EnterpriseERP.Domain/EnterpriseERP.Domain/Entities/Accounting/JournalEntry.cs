using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum JournalEntryStatus
{
    Draft = 0,
    Posted = 1    // Immutable once posted
}

public class JournalEntry : AuditableEntity, IAggregateRoot, ISoftDelete, ICompanyEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    
    public Guid CompanyId { get; set; }
    public EnterpriseERP.Domain.Entities.Settings.Company Company { get; set; } = null!;

    // Multi-Currency Support
    public string CurrencyCode { get; set; } = "EGP";
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    
    public bool IsReversed { get; set; } = false;
    public Guid? ReversedByEntryId { get; set; }

    // Data Integrity & Audit (Hash Chain)
    public string EntryHash { get; set; } = string.Empty;
    public string PreviousEntryHash { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

