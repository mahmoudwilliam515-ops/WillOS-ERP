using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Intercompany.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.CreateIntercompanyTransaction;

public class CreateIntercompanyTransactionCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateIntercompanyTransactionCommand, IntercompanyTransactionDto>
{
    public async Task<IntercompanyTransactionDto> Handle(CreateIntercompanyTransactionCommand request, CancellationToken cancellationToken)
    {
        var source = await unitOfWork.Repository<Company>().GetByIdAsync(request.SourceCompanyId);
        var target = await unitOfWork.Repository<Company>().GetByIdAsync(request.TargetCompanyId);

        if (source == null || target == null)
        {
            throw new AccountingDomainException("Both source and target companies must exist.");
        }

        var transaction = new IntercompanyTransaction
        {
            Id = Guid.NewGuid(),
            TransactionNumber = $"IC-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpperInvariant()}",
            SourceCompanyId = request.SourceCompanyId,
            TargetCompanyId = request.TargetCompanyId,
            TransactionDate = request.TransactionDate,
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            Amount = request.Amount,
            Description = request.Description,
            SourceJournalEntryId = request.SourceJournalEntryId,
            TargetJournalEntryId = request.TargetJournalEntryId,
            Status = IntercompanyTransactionStatus.Posted
        };

        await unitOfWork.Repository<IntercompanyTransaction>().AddAsync(transaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    internal static IntercompanyTransactionDto ToDto(IntercompanyTransaction transaction) => new()
    {
        Id = transaction.Id,
        TransactionNumber = transaction.TransactionNumber,
        SourceCompanyId = transaction.SourceCompanyId,
        TargetCompanyId = transaction.TargetCompanyId,
        TransactionDate = transaction.TransactionDate,
        CurrencyCode = transaction.CurrencyCode,
        Amount = transaction.Amount,
        Description = transaction.Description,
        Status = transaction.Status,
        MatchedAt = transaction.MatchedAt,
        EliminatedAt = transaction.EliminatedAt
    };
}
