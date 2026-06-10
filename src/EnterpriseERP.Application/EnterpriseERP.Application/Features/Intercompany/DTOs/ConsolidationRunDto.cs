using EnterpriseERP.Domain.Entities.Accounting;

namespace EnterpriseERP.Application.Features.Intercompany.DTOs;

public class ConsolidationRunDto
{
    public Guid Id { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public Guid GroupCompanyId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal IntercompanyAmount { get; set; }
    public decimal EliminatedAmount { get; set; }
    public decimal UnmatchedAmount { get; set; }
    public ConsolidationRunStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}
