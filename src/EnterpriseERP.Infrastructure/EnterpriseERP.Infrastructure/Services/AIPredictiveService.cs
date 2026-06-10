using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Services;

public class AIPredictiveService : IAIPredictiveService
{
    private readonly IUnitOfWork _unitOfWork;

    public AIPredictiveService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PredictionData>> PredictCashFlowAsync(int daysAhead)
    {
        // 1. Fetch historical cash flow data (last 6 months)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var history = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted && l.JournalEntry.EntryDate >= sixMonthsAgo)
            .Where(l => l.Account!.Name.Contains("Cash") || l.Account!.Name.Contains("Bank"))
            .GroupBy(l => l.JournalEntry.EntryDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                NetFlow = g.Sum(l => l.BaseDebitAmount - l.BaseCreditAmount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        if (history.Count < 2) return Enumerable.Empty<PredictionData>();

        // 2. Simple Linear Regression (AI Heuristic)
        // y = mx + b
        int n = history.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        
        for (int i = 0; i < n; i++)
        {
            double x = i;
            double y = (double)history[i].NetFlow;
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        double m = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        double b = (sumY - m * sumX) / n;

        // 3. Generate Predictions
        var predictions = new List<PredictionData>();
        var lastDate = history.Last().Date;

        for (int i = 1; i <= daysAhead; i++)
        {
            double nextX = n + i;
            decimal predictedY = (decimal)(m * nextX + b);
            predictions.Add(new PredictionData(lastDate.AddDays(i), predictedY));
        }

        return predictions;
    }

    public async Task<IEnumerable<InventoryPrediction>> PredictInventoryReplenishmentAsync()
    {
        var items = await _unitOfWork.Repository<Item>().FindAsync(i => i.IsActive && !i.IsService);
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        
        var predictions = new List<InventoryPrediction>();

        foreach (var item in items)
        {
            // 1. Get usage (OUT transactions) in last 30 days
            var usageTransactions = await _unitOfWork.Repository<InventoryTransaction>().FindAsync(t => 
                t.ItemId == item.Id && 
                t.TransactionDate >= thirtyDaysAgo && 
                (t.Type == TransactionType.SalesOut || t.Type == TransactionType.ManufacturingOut || t.Type == TransactionType.AdjustmentOut));

            var totalUsage = usageTransactions.Sum(t => Math.Abs(t.Quantity));
            var usagePerDay = totalUsage / 30m;

            // 2. Get current stock
            var currentStock = await _unitOfWork.Repository<InventoryTransaction>().Query()
                .Where(t => t.ItemId == item.Id)
                .SumAsync(t => t.Quantity);

            // 3. Calculate days until stock out
            int daysUntilStockOut = usagePerDay > 0 ? (int)(currentStock / usagePerDay) : 999;

            if (daysUntilStockOut < 30) // Predict if stock runs out within a month
            {
                predictions.Add(new InventoryPrediction(
                    item.Id,
                    item.NameAr ?? item.NameEn,
                    Math.Round(usagePerDay, 2),
                    daysUntilStockOut,
                    currentStock
                ));
            }
        }

        return predictions.OrderBy(p => p.DaysUntilStockOut);
    }
}
