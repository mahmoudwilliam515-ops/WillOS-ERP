using MediatR;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Reports.Queries;

public class GetCashFlowQuery : IRequest<CashFlowDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CashFlowDetailLineDto
{
    public string Section { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CashFlowDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal NetIncome { get; set; }
    public decimal WorkingCapitalChange { get; set; }
    public decimal OperatingCashFlow { get; set; }
    public decimal InvestingCashFlow { get; set; }
    public decimal FinancingCashFlow { get; set; }
    public decimal NetCashFlow { get; set; }
    public decimal OpeningCashBalance { get; set; }
    public decimal ClosingCashBalance { get; set; }
    public List<CashFlowDetailLineDto> Details { get; set; } = [];
}

public class GetCashFlowQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCashFlowQuery, CashFlowDto>
{
    public async Task<CashFlowDto> Handle(GetCashFlowQuery request, CancellationToken cancellationToken)
    {
        var postedInPeriod = unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.EntryDate >= request.StartDate
                        && l.JournalEntry.EntryDate <= request.EndDate
                        && l.JournalEntry.Status == JournalEntryStatus.Posted);

        var netIncome = await postedInPeriod
            .Where(l => l.Account!.Type == AccountType.Revenue || l.Account!.Type == AccountType.Expense)
            .SumAsync(l => l.CreditAmount - l.DebitAmount, cancellationToken);

        var workingCapitalLines = await postedInPeriod
            .Where(l => l.Account!.CashFlowCategory == CashFlowCategory.Operating)
            .GroupBy(l => new { l.AccountCode, l.Account!.Name })
            .Select(g => new CashFlowDetailLineDto
            {
                Section = "Operating",
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.Name,
                Amount = g.Sum(l => l.CreditAmount - l.DebitAmount)
            })
            .Where(x => x.Amount != 0)
            .ToListAsync(cancellationToken);

        var investingLines = await postedInPeriod
            .Where(l => l.Account!.CashFlowCategory == CashFlowCategory.Investing)
            .GroupBy(l => new { l.AccountCode, l.Account!.Name })
            .Select(g => new CashFlowDetailLineDto
            {
                Section = "Investing",
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.Name,
                Amount = g.Sum(l => l.CreditAmount - l.DebitAmount)
            })
            .Where(x => x.Amount != 0)
            .ToListAsync(cancellationToken);

        var financingLines = await postedInPeriod
            .Where(l => l.Account!.CashFlowCategory == CashFlowCategory.Financing)
            .GroupBy(l => new { l.AccountCode, l.Account!.Name })
            .Select(g => new CashFlowDetailLineDto
            {
                Section = "Financing",
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.Name,
                Amount = g.Sum(l => l.CreditAmount - l.DebitAmount)
            })
            .Where(x => x.Amount != 0)
            .ToListAsync(cancellationToken);

        var workingCapitalChange = workingCapitalLines.Sum(l => l.Amount);
        var investingCashFlow = investingLines.Sum(l => l.Amount);
        var financingCashFlow = financingLines.Sum(l => l.Amount);
        var operatingCashFlow = netIncome + workingCapitalChange;

        var cashAccounts = await unitOfWork.Repository<Account>().Query()
            .Where(a => a.Type == AccountType.Asset &&
                        (a.CashFlowCategory == CashFlowCategory.Operating ||
                         a.Code.StartsWith("11") ||
                         a.Name.Contains("نقد") || a.Name.Contains("بنك") ||
                         a.Name.ToLower().Contains("cash")))
            .Select(a => a.Code)
            .ToListAsync(cancellationToken);

        var openingCash = cashAccounts.Count == 0
            ? 0m
            : await unitOfWork.Repository<JournalEntryLine>().Query()
                .Where(l => l.JournalEntry.EntryDate < request.StartDate && l.JournalEntry.Status == JournalEntryStatus.Posted)
                .Where(l => cashAccounts.Contains(l.AccountCode))
                .SumAsync(l => l.DebitAmount - l.CreditAmount, cancellationToken);

        var closingCash = cashAccounts.Count == 0
            ? openingCash + operatingCashFlow + investingCashFlow + financingCashFlow
            : await unitOfWork.Repository<JournalEntryLine>().Query()
                .Where(l => l.JournalEntry.EntryDate <= request.EndDate && l.JournalEntry.Status == JournalEntryStatus.Posted)
                .Where(l => cashAccounts.Contains(l.AccountCode))
                .SumAsync(l => l.DebitAmount - l.CreditAmount, cancellationToken);

        var details = new List<CashFlowDetailLineDto>
        {
            new() { Section = "Operating", AccountCode = "—", AccountName = "Net income", Amount = netIncome }
        };
        details.AddRange(workingCapitalLines);
        details.AddRange(investingLines);
        details.AddRange(financingLines);

        return new CashFlowDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            NetIncome = netIncome,
            WorkingCapitalChange = workingCapitalChange,
            OperatingCashFlow = operatingCashFlow,
            InvestingCashFlow = investingCashFlow,
            FinancingCashFlow = financingCashFlow,
            NetCashFlow = operatingCashFlow + investingCashFlow + financingCashFlow,
            OpeningCashBalance = openingCash,
            ClosingCashBalance = closingCash,
            Details = details
        };
    }
}
