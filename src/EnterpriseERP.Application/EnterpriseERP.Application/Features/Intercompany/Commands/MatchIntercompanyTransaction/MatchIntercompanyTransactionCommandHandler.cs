using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.MatchIntercompanyTransaction;

public class MatchIntercompanyTransactionCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<MatchIntercompanyTransactionCommand, bool>
{
    public async Task<bool> Handle(MatchIntercompanyTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await unitOfWork.Repository<IntercompanyTransaction>().GetByIdAsync(request.TransactionId);
        if (transaction == null)
        {
            throw new AccountingDomainException("Intercompany transaction not found.");
        }

        if (transaction.Status == IntercompanyTransactionStatus.Eliminated)
        {
            throw new AccountingDomainException("Eliminated intercompany transactions cannot be matched again.");
        }

        transaction.Status = IntercompanyTransactionStatus.Matched;
        transaction.MatchedAt = DateTime.UtcNow;
        unitOfWork.Repository<IntercompanyTransaction>().Update(transaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
