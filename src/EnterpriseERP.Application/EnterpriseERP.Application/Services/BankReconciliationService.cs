using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Services;

public class BankReconciliationService : IBankReconciliationService
{
    private readonly IUnitOfWork _unitOfWork;

    public BankReconciliationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BankReconciliation> CreateReconciliationAsync(Guid bankAccountId, DateTime statementDate, decimal endingBalance)
    {
        var bankAccount = await _unitOfWork.Repository<BankAccount>().GetByIdAsync(bankAccountId);
        if (bankAccount == null) throw new AccountingDomainException("Bank account not found.");

        var reconciliation = new BankReconciliation
        {
            Id = Guid.NewGuid(),
            BankAccountId = bankAccountId,
            StatementDate = statementDate,
            StatementEndingBalance = endingBalance,
            IsCompleted = false,
            Lines = new List<BankReconciliationLine>()
        };

        await _unitOfWork.Repository<BankReconciliation>().AddAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync(default);

        return reconciliation;
    }

    public async Task<BankReconciliation> MatchLineAsync(Guid reconciliationId, Guid journalEntryLineId)
    {
        var recon = await _unitOfWork.Repository<BankReconciliation>().GetByIdAsync(reconciliationId);
        if (recon == null) throw new AccountingDomainException("Reconciliation not found.");
        if (recon.IsCompleted) throw new AccountingDomainException("Cannot modify a completed reconciliation.");

        var line = await _unitOfWork.Repository<JournalEntryLine>().GetByIdAsync(journalEntryLineId);
        if (line == null) throw new AccountingDomainException("Journal entry line not found.");

        // Check if already matched in another recon
        var existingMatch = await _unitOfWork.Repository<BankReconciliationLine>()
            .FindAsync(l => l.JournalEntryLineId == journalEntryLineId && l.IsMatched);
        
        if (existingMatch.Any())
            throw new AccountingDomainException("This transaction is already matched in another reconciliation.");

        var reconLine = new BankReconciliationLine
        {
            Id = Guid.NewGuid(),
            BankReconciliationId = reconciliationId,
            JournalEntryLineId = journalEntryLineId,
            IsMatched = true
        };

        recon.Lines.Add(reconLine);
        recon.TotalReconciled += (line.DebitAmount - line.CreditAmount);

        _unitOfWork.Repository<BankReconciliation>().Update(recon);
        await _unitOfWork.SaveChangesAsync(default);

        return recon;
    }

    public async Task<BankReconciliation> UnmatchLineAsync(Guid reconciliationId, Guid journalEntryLineId)
    {
        var recon = await _unitOfWork.Repository<BankReconciliation>().GetByIdAsync(reconciliationId);
        if (recon == null) throw new AccountingDomainException("Reconciliation not found.");
        if (recon.IsCompleted) throw new AccountingDomainException("Cannot modify a completed reconciliation.");

        var reconLine = recon.Lines.FirstOrDefault(l => l.JournalEntryLineId == journalEntryLineId);
        if (reconLine == null) throw new AccountingDomainException("Match not found in this reconciliation.");

        var line = await _unitOfWork.Repository<JournalEntryLine>().GetByIdAsync(journalEntryLineId);

        recon.Lines.Remove(reconLine);
        recon.TotalReconciled -= (line.DebitAmount - line.CreditAmount);

        _unitOfWork.Repository<BankReconciliation>().Update(recon);
        await _unitOfWork.SaveChangesAsync(default);

        return recon;
    }

    public async Task<BankReconciliation> CompleteReconciliationAsync(Guid reconciliationId)
    {
        var recon = await _unitOfWork.Repository<BankReconciliation>().GetByIdAsync(reconciliationId);
        if (recon == null) throw new AccountingDomainException("Reconciliation not found.");

        // In a real ERP, we would validate that TotalReconciled matches the difference between 
        // start and end balance, taking into account unpresented checks etc.
        // For simplicity:
        recon.IsCompleted = true;

        // Update Bank Account Statement Balance
        var bankAccount = await _unitOfWork.Repository<BankAccount>().GetByIdAsync(recon.BankAccountId);
        bankAccount.BankStatementBalance = recon.StatementEndingBalance;

        _unitOfWork.Repository<BankReconciliation>().Update(recon);
        _unitOfWork.Repository<BankAccount>().Update(bankAccount);
        await _unitOfWork.SaveChangesAsync(default);

        return recon;
    }

    public async Task<IEnumerable<JournalEntryLine>> GetUnreconciledLinesAsync(Guid bankAccountId)
    {
        var bankAccount = await _unitOfWork.Repository<BankAccount>().GetByIdAsync(bankAccountId);
        if (bankAccount == null || !bankAccount.LedgerAccountId.HasValue) 
            return Enumerable.Empty<JournalEntryLine>();

        // Find all JE lines for this bank's ledger account
        var allLines = await _unitOfWork.Repository<JournalEntryLine>()
            .FindAsync(l => l.AccountId == bankAccount.LedgerAccountId.Value);

        // Find all matched lines
        var matchedLineIds = (await _unitOfWork.Repository<BankReconciliationLine>()
            .FindAsync(l => l.IsMatched))
            .Select(l => l.JournalEntryLineId)
            .ToHashSet();

        return allLines.Where(l => !matchedLineIds.Contains(l.Id));
    }
}
