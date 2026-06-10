using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Reports.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Reports.Queries;

public record GetProfitAndLossQuery(DateTime FromDate, DateTime ToDate) : IRequest<ProfitAndLossReportDto>;

public class GetProfitAndLossQueryHandler : IRequestHandler<GetProfitAndLossQuery, ProfitAndLossReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProfitAndLossQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProfitAndLossReportDto> Handle(GetProfitAndLossQuery request, CancellationToken cancellationToken)
    {
        // 1. Database-side aggregation for all Revenue and Expense lines
        var groupedLines = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.Status == JournalEntryStatus.Posted &&
                        l.JournalEntry.EntryDate >= request.FromDate &&
                        l.JournalEntry.EntryDate <= request.ToDate)
            .Where(l => l.Account!.Type == AccountType.Revenue || l.Account!.Type == AccountType.Expense)
            .GroupBy(l => new { l.AccountCode, l.AccountName, l.Account!.Type })
            .Select(g => new
            {
                g.Key.AccountCode,
                g.Key.AccountName,
                g.Key.Type,
                Debit = g.Sum(l => l.DebitAmount),
                Credit = g.Sum(l => l.CreditAmount),
                Net = g.Sum(l => l.CreditAmount - l.DebitAmount)
            })
            .OrderBy(x => x.AccountCode)
            .ToListAsync(cancellationToken);

        // 2. Map to DTOs based on AccountType instead of hardcoded prefixes
        var revenueLines = groupedLines
            .Where(l => l.Type == AccountType.Revenue)
            .Select(l => new AccountLineDto
            {
                AccountCode = l.AccountCode,
                AccountName = l.AccountName,
                Debit = l.Debit,
                Credit = l.Credit,
                Net = l.Net
            }).ToList();

        // In this ERP, COGS are typically marked as Expenses. 
        // We'll separate COGS if they have a specific naming or just include in Expenses for now to avoid hardcoding.
        // For backward compatibility with the DTO, we'll put all non-revenue as expenses.
        var expenseLines = groupedLines
            .Where(l => l.Type == AccountType.Expense)
            .Select(l => new AccountLineDto
            {
                AccountCode = l.AccountCode,
                AccountName = l.AccountName,
                Debit = l.Debit,
                Credit = l.Credit,
                Net = Math.Abs(l.Net) // Expenses shown as positive in report
            }).ToList();

        var totalRevenue = revenueLines.Sum(l => l.Net);
        var totalExpenses = expenseLines.Sum(l => l.Net);
        var netProfit = totalRevenue - totalExpenses;

        return new ProfitAndLossReportDto
        {
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            RevenueLines = revenueLines,
            TotalRevenue = totalRevenue,
            CogsLines = new List<AccountLineDto>(), // COGS integrated into Expenses for architectural purity
            TotalCogs = 0,
            GrossProfit = totalRevenue, // Gross Profit = Total Revenue if COGS not separated
            ExpenseLines = expenseLines,
            TotalExpenses = totalExpenses,
            NetProfit = netProfit
        };
    }
}
