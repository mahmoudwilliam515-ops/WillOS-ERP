using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Reports.Queries.GetGlobalAnalytics;

public class GetGlobalAnalyticsQueryHandler : IRequestHandler<GetGlobalAnalyticsQuery, GlobalAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetGlobalAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GlobalAnalyticsDto> Handle(GetGlobalAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // 1. Get Base Currency Totals (SQL Side Aggregation)
        var pnlData = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted &&
                        l.JournalEntry.EntryDate >= request.FromDate &&
                        l.JournalEntry.EntryDate <= request.ToDate)
            .Where(l => l.Account!.Type == AccountType.Revenue || l.Account!.Type == AccountType.Expense)
            .GroupBy(l => l.Account!.Type)
            .Select(g => new
            {
                Type = g.Key,
                TotalBase = g.Sum(l => g.Key == AccountType.Revenue 
                    ? (l.BaseCreditAmount - l.BaseDebitAmount) 
                    : (l.BaseDebitAmount - l.BaseCreditAmount))
            })
            .ToListAsync(cancellationToken);

        var revenue = pnlData.FirstOrDefault(x => x.Type == AccountType.Revenue)?.TotalBase ?? 0;
        var expense = pnlData.FirstOrDefault(x => x.Type == AccountType.Expense)?.TotalBase ?? 0;

        // 2. Get Cash Balance in Base Currency
        var cashBalance = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted && l.JournalEntry.EntryDate <= request.ToDate)
            .Where(l => l.Account!.Name.Contains("Cash") || l.Account!.Name.Contains("Bank")) // Simplified
            .SumAsync(l => l.BaseDebitAmount - l.BaseCreditAmount, cancellationToken);

        // 3. Monthly Revenue Trend
        var trend = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted &&
                        l.JournalEntry.EntryDate >= request.FromDate &&
                        l.JournalEntry.EntryDate <= request.ToDate &&
                        l.Account!.Type == AccountType.Revenue)
            .GroupBy(l => new { l.JournalEntry.EntryDate.Year, l.JournalEntry.EntryDate.Month })
            .Select(g => new MonthlyTrendDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Amount = g.Sum(l => l.BaseCreditAmount - l.BaseDebitAmount)
            })
            .OrderBy(x => x.Month)
            .ToListAsync(cancellationToken);

        // 4. Top Customers
        var topCustomers = await _unitOfWork.Repository<SalesInvoice>().Query()
            .Where(i => i.InvoiceDate >= request.FromDate && i.InvoiceDate <= request.ToDate && i.Status == InvoiceStatus.Approved)
            .GroupBy(i => i.Customer.Name)
            .Select(g => new TopPerformanceDto
            {
                Name = g.Key,
                Value = g.Sum(i => i.TotalAmount)
            })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 5. Top Items
        var topItems = await _unitOfWork.Repository<SalesInvoiceLine>().Query()
            .Where(l => l.SalesInvoice.InvoiceDate >= request.FromDate && l.SalesInvoice.InvoiceDate <= request.ToDate && l.SalesInvoice.Status == InvoiceStatus.Approved)
            .GroupBy(l => l.Item.Name)
            .Select(g => new TopPerformanceDto
            {
                Name = g.Key,
                Value = g.Sum(l => l.LineTotal)
            })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new GlobalAnalyticsDto
        {
            TotalRevenueBase = revenue,
            TotalExpenseBase = expense,
            NetProfitBase = revenue - expense,
            CashBalanceBase = cashBalance,
            RevenueTrend = trend,
            TopCustomers = topCustomers,
            TopItems = topItems
        };
    }
}
