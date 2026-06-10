using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Services;

/// <summary>
/// Recomputes the party balance snapshot by aggregating all journal lines
/// tagged with the given PartyId. This is called after invoice approval/reversal
/// to maintain a fast, always-current balance cache.
/// </summary>
public class PartyBalanceService : IPartyBalanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public PartyBalanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task RecalculateAsync(Guid partyId, string partyType, CancellationToken cancellationToken = default)
    {
        // 1. Aggregate all posted journal lines for this party
        var lines = await _unitOfWork.Repository<JournalEntryLine>()
            .FindAsync(l => l.PartyId == partyId);

        // Filter to posted entries only
        var journalIds = lines.Select(l => l.JournalEntryId).Distinct().ToList();
        var entries = await _unitOfWork.Repository<JournalEntry>()
            .FindAsync(j => journalIds.Contains(j.Id) && j.Status == JournalEntryStatus.Posted && !j.IsReversed);

        var postedJournalIds = entries.Select(e => e.Id).ToHashSet();
        var postedLines = lines.Where(l => postedJournalIds.Contains(l.JournalEntryId)).ToList();

        var totalDebit = postedLines.Sum(l => l.DebitAmount);
        var totalCredit = postedLines.Sum(l => l.CreditAmount);

        // 2. Upsert PartyBalance record
        var existing = (await _unitOfWork.Repository<PartyBalance>()
            .FindAsync(pb => pb.PartyId == partyId)).FirstOrDefault();

        var pt = Enum.TryParse<PartyType>(partyType, out var parsedType) ? parsedType : PartyType.Customer;

        if (existing == null)
        {
            var newBalance = new PartyBalance
            {
                Id = Guid.NewGuid(),
                PartyId = partyId,
                PartyType = pt,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                LastUpdated = DateTime.UtcNow
            };
            await _unitOfWork.Repository<PartyBalance>().AddAsync(newBalance);
        }
        else
        {
            existing.TotalDebit = totalDebit;
            existing.TotalCredit = totalCredit;
            existing.LastUpdated = DateTime.UtcNow;
            _unitOfWork.Repository<PartyBalance>().Update(existing);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
