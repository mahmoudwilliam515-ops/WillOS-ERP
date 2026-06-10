using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using TreasuryEntities = EnterpriseERP.Domain.Entities.Treasury;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Treasury.BankReconciliations.Queries.GetUnreconciledTransactions;

public record GetUnreconciledTransactionsQuery(Guid BankAccountId) : IRequest<List<UnreconciledTransactionDto>>;

public class UnreconciledTransactionDto
{
    public Guid TransactionId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; } // Positive for Debit, Negative for Credit
    public string ReferenceNumber { get; set; } = string.Empty;
}

public class GetUnreconciledTransactionsQueryHandler : IRequestHandler<GetUnreconciledTransactionsQuery, List<UnreconciledTransactionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreconciledTransactionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UnreconciledTransactionDto>> Handle(GetUnreconciledTransactionsQuery request, CancellationToken cancellationToken)
    {
        var bankAccount = await _unitOfWork.Repository<TreasuryEntities.BankAccount>().GetByIdAsync(request.BankAccountId);
        if (bankAccount == null) return new List<UnreconciledTransactionDto>();

        // 1. Get all matched transaction IDs
        var matchedTransactionIds = await _unitOfWork.Repository<TreasuryEntities.BankReconciliationLine>().Query()
            .Where(l => l.IsCleared)
            .Select(l => l.TransactionId)
            .ToListAsync(cancellationToken);

        // 2. Get all JE lines for the linked account that are NOT matched
        var unreconciledLines = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Include(l => l.JournalEntry)
            .Where(l => l.AccountId == bankAccount.LinkedAccountId && !matchedTransactionIds.Contains(l.Id))
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ToListAsync(cancellationToken);

        return unreconciledLines.Select(l => new UnreconciledTransactionDto
        {
            TransactionId = l.Id,
            Date = l.JournalEntry.EntryDate,
            Description = l.Description ?? l.JournalEntry.Description,
            Amount = l.DebitAmount - l.CreditAmount,
            ReferenceNumber = l.JournalEntry.EntryNumber
        }).ToList();
    }
}
