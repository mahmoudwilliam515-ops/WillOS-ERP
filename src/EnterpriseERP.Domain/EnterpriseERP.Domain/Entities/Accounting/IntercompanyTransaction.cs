using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum IntercompanyTransactionStatus
{
    Draft = 0,
    Posted = 1,
    Matched = 2,
    Eliminated = 3,
    Disputed = 4
}

public class IntercompanyTransaction : AuditableEntity, IAggregateRoot
{
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid SourceCompanyId { get; set; }
    public Company SourceCompany { get; set; } = null!;
    public Guid TargetCompanyId { get; set; }
    public Company TargetCompany { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? SourceJournalEntryId { get; set; }
    public Guid? TargetJournalEntryId { get; set; }
    public IntercompanyTransactionStatus Status { get; set; } = IntercompanyTransactionStatus.Draft;
    public DateTime? MatchedAt { get; set; }
    public DateTime? EliminatedAt { get; set; }
    public Guid? ConsolidationRunId { get; set; }
}
