using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Intercompany.Commands.CreateIntercompanyTransaction;
using EnterpriseERP.Application.Features.Intercompany.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Queries.GetIntercompanyTransactions;

public class GetIntercompanyTransactionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetIntercompanyTransactionsQuery, IEnumerable<IntercompanyTransactionDto>>
{
    public async Task<IEnumerable<IntercompanyTransactionDto>> Handle(GetIntercompanyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await unitOfWork.Repository<IntercompanyTransaction>().GetAllAsync();

        return transactions
            .Where(t => !request.Status.HasValue || t.Status == request.Status.Value)
            .Where(t => !request.From.HasValue || t.TransactionDate >= request.From.Value)
            .Where(t => !request.To.HasValue || t.TransactionDate <= request.To.Value)
            .OrderByDescending(t => t.TransactionDate)
            .Select(CreateIntercompanyTransactionCommandHandler.ToDto)
            .ToList();
    }
}
