using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Reports.Queries;

public class GetBalanceSheetQuery : IRequest<BalanceSheetDto>
{
    public DateTime AsOfDate { get; set; }
}

public class AccountBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class BalanceSheetSectionDto
{
    public string Title { get; set; } = string.Empty;
    public List<AccountBalanceDto> Lines { get; set; } = [];
    public decimal Total { get; set; }
}

public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal CurrentPeriodNetIncome { get; set; }
    public bool IsBalanced { get; set; }
    public decimal BalanceDifference { get; set; }

    public BalanceSheetSectionDto CurrentAssets { get; set; } = new();
    public BalanceSheetSectionDto NonCurrentAssets { get; set; } = new();
    public BalanceSheetSectionDto CurrentLiabilities { get; set; } = new();
    public BalanceSheetSectionDto NonCurrentLiabilities { get; set; } = new();
    public BalanceSheetSectionDto Equity { get; set; } = new();

    // Legacy flat lists (backward compatible)
    public List<AccountBalanceDto> Assets { get; set; } = [];
    public List<AccountBalanceDto> Liabilities { get; set; } = [];
    public List<AccountBalanceDto> EquityAccounts { get; set; } = [];
}

public class GetBalanceSheetQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetBalanceSheetQuery, BalanceSheetDto>
{
    public async Task<BalanceSheetDto> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
    {
        var balances = await unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.EntryDate <= request.AsOfDate && l.JournalEntry.Status == JournalEntryStatus.Posted)
            .GroupBy(l => l.AccountCode)
            .Select(g => new
            {
                AccountCode = g.Key,
                Debit = g.Sum(l => l.DebitAmount),
                Credit = g.Sum(l => l.CreditAmount)
            })
            .ToListAsync(cancellationToken);

        var balanceDict = balances.ToDictionary(b => b.AccountCode);

        var accounts = await unitOfWork.Repository<Account>().Query()
            .Where(a => a.Type == AccountType.Asset || a.Type == AccountType.Liability || a.Type == AccountType.Equity)
            .ToListAsync(cancellationToken);

        var accountBalances = accounts
            .Select(a =>
            {
                decimal debit = 0, credit = 0;
                if (balanceDict.TryGetValue(a.Code, out var bal))
                {
                    debit = bal.Debit;
                    credit = bal.Credit;
                }

                decimal finalBalance = a.Type switch
                {
                    AccountType.Asset => debit - credit,
                    AccountType.Liability or AccountType.Equity => credit - debit,
                    _ => 0
                };

                return new AccountBalanceDto
                {
                    AccountId = a.Id,
                    AccountCode = a.Code,
                    AccountName = a.Name,
                    AccountType = a.Type.ToString(),
                    Classification = BalanceSheetClassificationHelper.Resolve(a).ToString(),
                    Balance = finalBalance
                };
            })
            .Where(a => a.Balance != 0)
            .ToList();

        var fiscalYearStart = new DateTime(request.AsOfDate.Year, 1, 1);
        var currentPeriodNetIncome = await unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.EntryDate >= fiscalYearStart
                        && l.JournalEntry.EntryDate <= request.AsOfDate
                        && l.JournalEntry.Status == JournalEntryStatus.Posted)
            .Where(l => l.Account!.Type == AccountType.Revenue || l.Account!.Type == AccountType.Expense)
            .SumAsync(l => l.CreditAmount - l.DebitAmount, cancellationToken);

        var assets = accountBalances.Where(a => a.AccountType == AccountType.Asset.ToString()).ToList();
        var liabilities = accountBalances.Where(a => a.AccountType == AccountType.Liability.ToString()).ToList();
        var equity = accountBalances.Where(a => a.AccountType == AccountType.Equity.ToString()).ToList();

        if (Math.Abs(currentPeriodNetIncome) > 0.001m)
        {
            equity.Add(new AccountBalanceDto
            {
                AccountCode = "YTD",
                AccountName = "Current period result (YTD)",
                AccountType = AccountType.Equity.ToString(),
                Classification = BalanceSheetClassification.NotApplicable.ToString(),
                Balance = currentPeriodNetIncome
            });
        }

        var currentAssets = assets.Where(a => a.Classification == BalanceSheetClassification.Current.ToString()).ToList();
        var nonCurrentAssets = assets.Where(a => a.Classification == BalanceSheetClassification.NonCurrent.ToString()).ToList();
        var unclassifiedAssets = assets.Where(a => a.Classification == BalanceSheetClassification.NotApplicable.ToString()).ToList();
        nonCurrentAssets.AddRange(unclassifiedAssets);

        var currentLiabilities = liabilities.Where(a => a.Classification == BalanceSheetClassification.Current.ToString()).ToList();
        var nonCurrentLiabilities = liabilities.Where(a => a.Classification == BalanceSheetClassification.NonCurrent.ToString()).ToList();
        var unclassifiedLiabilities = liabilities.Where(a => a.Classification == BalanceSheetClassification.NotApplicable.ToString()).ToList();
        nonCurrentLiabilities.AddRange(unclassifiedLiabilities);

        var totalAssets = assets.Sum(a => a.Balance);
        var totalLiabilities = liabilities.Sum(a => a.Balance);
        var totalEquity = equity.Sum(a => a.Balance);
        var difference = totalAssets - (totalLiabilities + totalEquity);

        return new BalanceSheetDto
        {
            AsOfDate = request.AsOfDate,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            CurrentPeriodNetIncome = currentPeriodNetIncome,
            IsBalanced = Math.Abs(difference) < 0.05m,
            BalanceDifference = difference,
            CurrentAssets = Section("Current Assets", currentAssets),
            NonCurrentAssets = Section("Non-Current Assets", nonCurrentAssets),
            CurrentLiabilities = Section("Current Liabilities", currentLiabilities),
            NonCurrentLiabilities = Section("Non-Current Liabilities", nonCurrentLiabilities),
            Equity = Section("Equity", equity),
            Assets = assets.OrderBy(a => a.AccountCode).ToList(),
            Liabilities = liabilities.OrderBy(a => a.AccountCode).ToList(),
            EquityAccounts = equity.OrderBy(a => a.AccountCode).ToList()
        };
    }

    private static BalanceSheetSectionDto Section(string title, List<AccountBalanceDto> lines) => new()
    {
        Title = title,
        Lines = lines.OrderBy(l => l.AccountCode).ToList(),
        Total = lines.Sum(l => l.Balance)
    };
}
