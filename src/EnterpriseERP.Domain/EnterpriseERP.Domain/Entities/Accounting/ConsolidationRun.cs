using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum ConsolidationRunStatus
{
    Draft = 0,
    Completed = 1,
    Reversed = 2
}

public class ConsolidationRun : AuditableEntity, IAggregateRoot
{
    public string RunNumber { get; set; } = string.Empty;
    public Guid GroupCompanyId { get; set; }
    public Company GroupCompany { get; set; } = null!;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal IntercompanyAmount { get; set; }
    public decimal EliminatedAmount { get; set; }
    public decimal UnmatchedAmount { get; set; }
    public ConsolidationRunStatus Status { get; set; } = ConsolidationRunStatus.Draft;
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}
