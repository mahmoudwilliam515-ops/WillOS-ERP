using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public record AnomalyResult(bool IsAnomaly, string Reason, double ConfidenceScore);

public interface IAIGovernanceService
{
    Task<AnomalyResult> AnalyzeJournalEntryAsync(Guid journalEntryId);
    Task<IEnumerable<AnomalyResult>> ScanForAnomaliesAsync(DateTime fromDate, DateTime toDate);
}
