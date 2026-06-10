using EnterpriseERP.Application.Features.Intercompany.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Queries.GetIntercompanyTransactions;

public class GetIntercompanyTransactionsQuery : IRequest<IEnumerable<IntercompanyTransactionDto>>
{
    public IntercompanyTransactionStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
