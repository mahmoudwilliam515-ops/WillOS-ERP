using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class ConsolidationService : IConsolidationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPostingService _postingService;

    public ConsolidationService(IUnitOfWork unitOfWork, IAccountingPostingService postingService)
    {
        _unitOfWork = unitOfWork;
        _postingService = postingService;
    }

    public async Task<Guid> RunConsolidationAsync(Guid groupCompanyId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default)
    {
        var groupCompany = await _unitOfWork.Repository<Company>().GetByIdAsync(groupCompanyId);
        if (groupCompany == null || !groupCompany.IsConsolidationEntity)
            throw new InvalidOperationException("Company is not a consolidation entity.");

        // 1. Identify subsidiaries
        var subsidiaries = await _unitOfWork.Repository<Company>().FindAsync(c => c.ParentCompanyId == groupCompanyId);
        if (!subsidiaries.Any())
            throw new InvalidOperationException("No subsidiaries found for consolidation.");

        var subsidiaryIds = subsidiaries.Select(s => s.Id).ToList();

        // 2. Aggregate Trial Balances for all subsidiaries
        // We group by Account Code to merge similar accounts across companies
        var balances = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => subsidiaryIds.Contains(l.CompanyId) && 
                        l.JournalEntry.Status == JournalEntryStatus.Posted &&
                        l.JournalEntry.EntryDate >= periodStart && 
                        l.JournalEntry.EntryDate <= periodEnd)
            .GroupBy(l => l.AccountCode)
            .Select(g => new
            {
                AccountCode = g.Key,
                AccountName = g.First().AccountName, // Take first name found
                Debit = g.Sum(l => l.DebitAmount),
                Credit = g.Sum(l => l.CreditAmount)
            })
            .ToListAsync(cancellationToken);

        // 3. Identify Elimination Entries (Intercompany Balances)
        // We look for IntercompanyDueTo and IntercompanyDueFrom accounts
        var intercompanyMappings = await _unitOfWork.Repository<AccountMapping>().Query()
            .Where(m => m.PostingKey == PostingKey.INTERCOMPANY_DUE_TO || m.PostingKey == PostingKey.INTERCOMPANY_DUE_FROM)
            .ToListAsync(cancellationToken);
        
        var intercompanyAccountIds = intercompanyMappings.Select(m => m.AccountId).ToList();
        var intercompanyAccounts = await _unitOfWork.Repository<Account>().FindAsync(a => intercompanyAccountIds.Contains(a.Id));
        var intercompanyAccountCodes = intercompanyAccounts.Select(a => a.Code).ToList();

        var intercompanyTotal = balances
            .Where(b => intercompanyAccountCodes.Contains(b.AccountCode))
            .Sum(b => b.Debit - b.Credit);

        // 4. Create Consolidation Journal Entry in Group Company
        var consolidationRun = new ConsolidationRun
        {
            Id = Guid.NewGuid(),
            RunNumber = $"CON-{DateTime.UtcNow:yyyyMMddHHmm}",
            GroupCompanyId = groupCompanyId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            IntercompanyAmount = balances.Where(b => intercompanyAccountCodes.Contains(b.AccountCode)).Sum(b => Math.Abs(b.Debit - b.Credit)),
            EliminatedAmount = Math.Abs(intercompanyTotal),
            Status = ConsolidationRunStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            Notes = $"Automated consolidation run for {subsidiaries.Count()} subsidiaries."
        };

        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            CompanyId = groupCompanyId,
            EntryNumber = $"JE-{consolidationRun.RunNumber}",
            EntryDate = periodEnd,
            Description = $"Consolidation Entry for period {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}",
            ReferenceId = consolidationRun.Id,
            ReferenceType = "ConsolidationRun",
            ReferenceNumber = consolidationRun.RunNumber,
            Status = JournalEntryStatus.Posted,
            Lines = new List<JournalEntryLine>()
        };

        foreach (var bal in balances)
        {
            // Skip intercompany accounts in the consolidated view (Elimination)
            if (intercompanyAccountCodes.Contains(bal.AccountCode)) continue;

            if (bal.Debit != 0 || bal.Credit != 0)
            {
                journalEntry.Lines.Add(new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    CompanyId = groupCompanyId,
                    AccountCode = bal.AccountCode,
                    AccountName = bal.AccountName,
                    DebitAmount = bal.Debit,
                    CreditAmount = bal.Credit,
                    Description = $"Consolidated balance from subsidiaries"
                });
            }
        }

        // Add elimination variance if any to a suspense/reconciliation account if needed
        // For now, we assume subsidiaries balance out.
        
        journalEntry.TotalDebit = journalEntry.Lines.Sum(l => l.DebitAmount);
        journalEntry.TotalCredit = journalEntry.Lines.Sum(l => l.CreditAmount);

        await _unitOfWork.Repository<ConsolidationRun>().AddAsync(consolidationRun);
        await _unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);
        await _unitOfWork.SaveChangesAsync();

        return consolidationRun.Id;
    }
}
