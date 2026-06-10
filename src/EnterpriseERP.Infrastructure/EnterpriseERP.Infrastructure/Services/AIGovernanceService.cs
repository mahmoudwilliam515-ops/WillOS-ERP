using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Services;

public class AIGovernanceService : IAIGovernanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AIGovernanceService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<AnomalyResult> AnalyzeJournalEntryAsync(Guid journalEntryId)
    {
        var entry = await _unitOfWork.Repository<JournalEntry>().Query()
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == journalEntryId);

        if (entry == null) return new AnomalyResult(false, "Entry not found", 0);

        AnomalyResult? result = null;

        // Rule 1: Amount Outlier Detection
        var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
        var averageAmount = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.EntryDate >= threeMonthsAgo)
            .AverageAsync(l => (double?)l.DebitAmount) ?? 0;

        var entryMaxLine = entry.Lines.Max(l => (double)l.DebitAmount);
        
        if (entryMaxLine > averageAmount * 10)
        {
            result = new AnomalyResult(true, $"Amount Outlier: Maximum line amount ({entryMaxLine}) is significantly higher than 3-month average ({averageAmount}).", 0.95);
        }

        // Rule 2: Unusual Posting Time
        if (result == null)
        {
            var entryTime = entry.EntryDate;
            if (entryTime.DayOfWeek == DayOfWeek.Saturday || entryTime.DayOfWeek == DayOfWeek.Sunday)
            {
                result = new AnomalyResult(true, "Unusual Posting Date: Posted on a weekend.", 0.70);
            }
            else if (entryTime.Hour >= 23 || entryTime.Hour <= 5)
            {
                result = new AnomalyResult(true, "Unusual Posting Time: Posted during non-business hours (late night).", 0.85);
            }
        }

        if (result != null && result.IsAnomaly)
        {
            await _notificationService.SendSystemAlertAsync(
                "AI Governance Alert", 
                $"Suspicious activity detected in Journal Entry {entry.EntryNumber}. Reason: {result.Reason}", 
                result.ConfidenceScore > 0.9 ? "Critical" : "Warning"
            );
            return result;
        }

        return new AnomalyResult(false, "No anomalies detected by AI heuristics.", 0.1);
    }

    public async Task<IEnumerable<AnomalyResult>> ScanForAnomaliesAsync(DateTime fromDate, DateTime toDate)
    {
        var entries = await _unitOfWork.Repository<JournalEntry>().Query()
            .Where(e => e.EntryDate >= fromDate && e.EntryDate <= toDate)
            .Select(e => e.Id)
            .ToListAsync();

        var results = new List<AnomalyResult>();
        foreach (var id in entries)
        {
            var result = await AnalyzeJournalEntryAsync(id);
            if (result.IsAnomaly)
            {
                results.Add(result);
            }
        }

        return results;
    }
}
