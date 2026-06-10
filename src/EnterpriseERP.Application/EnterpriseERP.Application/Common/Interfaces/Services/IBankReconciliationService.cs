using EnterpriseERP.Domain.Entities.Accounting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IBankReconciliationService
{
    Task<BankReconciliation> CreateReconciliationAsync(Guid bankAccountId, DateTime statementDate, decimal endingBalance);
    Task<BankReconciliation> MatchLineAsync(Guid reconciliationId, Guid journalEntryLineId);
    Task<BankReconciliation> UnmatchLineAsync(Guid reconciliationId, Guid journalEntryLineId);
    Task<BankReconciliation> CompleteReconciliationAsync(Guid reconciliationId);
    Task<IEnumerable<JournalEntryLine>> GetUnreconciledLinesAsync(Guid bankAccountId);
}
