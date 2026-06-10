using EnterpriseERP.Application.Features.Intercompany.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.CreateIntercompanyTransaction;

public class CreateIntercompanyTransactionCommand : IRequest<IntercompanyTransactionDto>
{
    public Guid SourceCompanyId { get; set; }
    public Guid TargetCompanyId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? SourceJournalEntryId { get; set; }
    public Guid? TargetJournalEntryId { get; set; }
}
