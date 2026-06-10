using EnterpriseERP.Domain.Entities.Accounting;

namespace EnterpriseERP.Application.Features.Intercompany.DTOs;

public class IntercompanyTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public Guid SourceCompanyId { get; set; }
    public Guid TargetCompanyId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public IntercompanyTransactionStatus Status { get; set; }
    public DateTime? MatchedAt { get; set; }
    public DateTime? EliminatedAt { get; set; }
}
