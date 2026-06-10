using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.Commands.ReverseJournalEntry;

public class ReverseJournalEntryCommandHandler : IRequestHandler<ReverseJournalEntryCommand, JournalEntryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReverseJournalEntryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntryDto> Handle(ReverseJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var originalEntry = await _unitOfWork.Repository<JournalEntry>().GetByIdAsync(request.OriginalJournalEntryId);
        if (originalEntry == null)
            throw new InvalidOperationException("Original journal entry not found.");

        if (originalEntry.IsReversed)
            throw new InvalidOperationException("Journal entry is already reversed.");

        // Check if period is open
        var periods = await _unitOfWork.Repository<AccountingPeriod>()
            .FindAsync(p => p.StartDate.Date <= request.ReversalDate.Date && p.EndDate.Date >= request.ReversalDate.Date);
            
        var targetPeriod = periods.FirstOrDefault();
        if (targetPeriod == null)
            throw new InvalidOperationException($"No accounting period found for the date {request.ReversalDate:yyyy-MM-dd}.");
            
        if (targetPeriod.Status == EnterpriseERP.Domain.Entities.Accounting.AccountingPeriodStatus.Closed)
            throw new InvalidOperationException($"The accounting period '{targetPeriod.PeriodName}' is closed. Cannot post reversal entries to a closed period.");

        var entryNumber = $"REV-{originalEntry.EntryNumber}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

        // Swap debits and credits
        var lines = originalEntry.Lines.Select(l => new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            AccountCode = l.AccountCode,
            AccountName = l.AccountName,
            DebitAmount = l.CreditAmount, // SWAPPED
            CreditAmount = l.DebitAmount, // SWAPPED
            Description = $"Reversal of {originalEntry.EntryNumber}: {l.Description}"
        }).ToList();

        var reversalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = entryNumber,
            EntryDate = request.ReversalDate,
            Description = $"Reversal of {originalEntry.EntryNumber}. Reason: {request.Reason}",
            ReferenceType = originalEntry.ReferenceType,
            ReferenceNumber = originalEntry.ReferenceNumber + "-REV",
            TotalDebit = originalEntry.TotalDebit,
            TotalCredit = originalEntry.TotalCredit,
            Status = JournalEntryStatus.Posted,
            Lines = lines
        };

        originalEntry.IsReversed = true;
        originalEntry.ReversedByEntryId = reversalEntry.Id;

        await _unitOfWork.Repository<JournalEntry>().AddAsync(reversalEntry);
        _unitOfWork.Repository<JournalEntry>().Update(originalEntry);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new JournalEntryDto
        {
            Id = reversalEntry.Id,
            EntryNumber = reversalEntry.EntryNumber,
            EntryDate = reversalEntry.EntryDate,
            Description = reversalEntry.Description,
            ReferenceType = reversalEntry.ReferenceType,
            ReferenceNumber = reversalEntry.ReferenceNumber,
            TotalDebit = reversalEntry.TotalDebit,
            TotalCredit = reversalEntry.TotalCredit,
            Status = reversalEntry.Status.ToString(),
            Lines = reversalEntry.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountCode = l.AccountCode,
                AccountName = l.AccountName,
                DebitAmount = l.DebitAmount,
                CreditAmount = l.CreditAmount,
                Description = l.Description
            }).ToList()
        };
    }
}
