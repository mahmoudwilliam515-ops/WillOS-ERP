using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Reports;

// ═══════════════════════════════════════════════════════════════
// Balance Sheet — الميزانية العمومية
// Blueprint Section 1.3 — Record-to-Report
// ═══════════════════════════════════════════════════════════════

public record GetBalanceSheetQuery : IRequest<BalanceSheetDto>
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
    public DateTime? AsOfDate { get; init; }  // null = نهاية الفترة
}

public class GetBalanceSheetQueryHandler : IRequestHandler<GetBalanceSheetQuery, BalanceSheetDto>
{
    private readonly IAppDbContext _context;

    public GetBalanceSheetQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<BalanceSheetDto> Handle(GetBalanceSheetQuery query, CancellationToken cancellationToken)
    {
        var period = await _context.AccountingPeriods
            .FirstOrDefaultAsync(p => p.Id == query.PeriodId && p.CompanyId == query.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Accounting period {query.PeriodId} not found");

        var asOfDate = query.AsOfDate ?? period.EndDate;

        // جلب أرصدة GL مجمعة حسب نوع الحساب
        var glBalances = await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == query.CompanyId && e.PostingDate <= asOfDate)
            .GroupBy(e => new { e.AccountCode, e.AccountType, e.AccountName, e.AccountCategory })
            .Select(g => new AccountBalance
            {
                AccountCode = g.Key.AccountCode,
                AccountName = g.Key.AccountName,
                AccountType = g.Key.AccountType,
                AccountCategory = g.Key.AccountCategory,
                DebitTotal = g.Sum(e => e.DebitAmount),
                CreditTotal = g.Sum(e => e.CreditAmount),
                NetBalance = g.Sum(e => e.DebitAmount - e.CreditAmount)
            })
            .ToListAsync(cancellationToken);

        // تجميع الأصول
        var assets = glBalances
            .Where(b => b.AccountType == AccountType.Asset)
            .GroupBy(b => b.AccountCategory)
            .Select(g => new BalanceSheetSection
            {
                Category = g.Key,
                Lines = g.Select(b => new BalanceSheetLine
                {
                    AccountCode = b.AccountCode,
                    AccountName = b.AccountName,
                    Amount = b.NetBalance  // الأصول: رصيد مدين = موجب
                }).OrderBy(l => l.AccountCode).ToList(),
                Total = g.Sum(b => b.NetBalance)
            })
            .OrderBy(s => s.Category)
            .ToList();

        // تجميع الالتزامات
        var liabilities = glBalances
            .Where(b => b.AccountType == AccountType.Liability)
            .GroupBy(b => b.AccountCategory)
            .Select(g => new BalanceSheetSection
            {
                Category = g.Key,
                Lines = g.Select(b => new BalanceSheetLine
                {
                    AccountCode = b.AccountCode,
                    AccountName = b.AccountName,
                    Amount = -b.NetBalance  // الالتزامات: رصيد دائن = موجب (عكس الإشارة)
                }).OrderBy(l => l.AccountCode).ToList(),
                Total = -g.Sum(b => b.NetBalance)
            })
            .OrderBy(s => s.Category)
            .ToList();

        // تجميع حقوق الملكية
        var equity = glBalances
            .Where(b => b.AccountType == AccountType.Equity)
            .GroupBy(b => b.AccountCategory)
            .Select(g => new BalanceSheetSection
            {
                Category = g.Key,
                Lines = g.Select(b => new BalanceSheetLine
                {
                    AccountCode = b.AccountCode,
                    AccountName = b.AccountName,
                    Amount = -b.NetBalance
                }).OrderBy(l => l.AccountCode).ToList(),
                Total = -g.Sum(b => b.NetBalance)
            })
            .OrderBy(s => s.Category)
            .ToList();

        var totalAssets = assets.Sum(s => s.Total);
        var totalLiabilities = liabilities.Sum(s => s.Total);
        var totalEquity = equity.Sum(s => s.Total);

        // التحقق من معادلة الميزانية — القاعدة الذهبية
        // Assets = Liabilities + Equity
        var balanceDifference = totalAssets - (totalLiabilities + totalEquity);
        if (Math.Abs(balanceDifference) > 0.01m)
        {
            // لا نرفع Exception بل نُسجِّل الفرق للمراجعة
            // يجب أن يكون الفرق صفراً في نظام صحيح
        }

        return new BalanceSheetDto
        {
            CompanyId = query.CompanyId,
            PeriodId = query.PeriodId,
            AsOfDate = asOfDate,
            Assets = assets,
            Liabilities = liabilities,
            Equity = equity,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            IsBalanced = Math.Abs(balanceDifference) <= 0.01m,
            BalanceDifference = balanceDifference
        };
    }
}

// ═══════════════════════════════════════════════════════════════
// Cash Flow Statement — قائمة التدفقات النقدية (الطريقة غير المباشرة)
// Blueprint Section 1.3
// ═══════════════════════════════════════════════════════════════

public record GetCashFlowQuery : IRequest<CashFlowDto>
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
}

public class GetCashFlowQueryHandler : IRequestHandler<GetCashFlowQuery, CashFlowDto>
{
    private readonly IAppDbContext _context;

    public GetCashFlowQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CashFlowDto> Handle(GetCashFlowQuery query, CancellationToken cancellationToken)
    {
        var period = await _context.AccountingPeriods
            .FirstOrDefaultAsync(p => p.Id == query.PeriodId && p.CompanyId == query.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Period {query.PeriodId} not found");

        // ─── الطريقة غير المباشرة ────────────────────────────────────

        // 1. صافي الدخل من P&L
        var netIncome = await GetNetIncomeAsync(query.CompanyId, period, cancellationToken);

        // 2. التسويات (بنود غير نقدية)
        var depreciation = await GetDepreciationAsync(query.CompanyId, period, cancellationToken);
        var amortization = await GetAmortizationAsync(query.CompanyId, period, cancellationToken);

        // 3. التغيرات في رأس المال العامل
        var arChange = await GetARChangeAsync(query.CompanyId, period, cancellationToken);
        var inventoryChange = await GetInventoryChangeAsync(query.CompanyId, period, cancellationToken);
        var apChange = await GetAPChangeAsync(query.CompanyId, period, cancellationToken);
        var otherWorkingCapitalChange = await GetOtherWorkingCapitalChangeAsync(query.CompanyId, period, cancellationToken);

        // 4. الأنشطة الاستثمارية
        var capitalExpenditures = await GetCapExAsync(query.CompanyId, period, cancellationToken);
        var assetDisposals = await GetAssetDisposalsAsync(query.CompanyId, period, cancellationToken);

        // 5. الأنشطة التمويلية
        var loanProceeds = await GetLoanProceedsAsync(query.CompanyId, period, cancellationToken);
        var loanRepayments = await GetLoanRepaymentsAsync(query.CompanyId, period, cancellationToken);
        var dividendsPaid = await GetDividendsPaidAsync(query.CompanyId, period, cancellationToken);

        // الحساب
        var operatingCashFlow = netIncome + depreciation + amortization
                                - arChange - inventoryChange + apChange + otherWorkingCapitalChange;
        var investingCashFlow = -capitalExpenditures + assetDisposals;
        var financingCashFlow = loanProceeds - loanRepayments - dividendsPaid;
        var netCashChange = operatingCashFlow + investingCashFlow + financingCashFlow;

        return new CashFlowDto
        {
            CompanyId = query.CompanyId,
            PeriodId = query.PeriodId,
            PeriodName = period.PeriodName,
            StartDate = period.StartDate,
            EndDate = period.EndDate,

            // الأنشطة التشغيلية
            NetIncome = netIncome,
            Depreciation = depreciation,
            Amortization = amortization,
            ARIncrease = -arChange,
            InventoryIncrease = -inventoryChange,
            APIncrease = apChange,
            OtherWorkingCapitalChange = otherWorkingCapitalChange,
            OperatingCashFlow = operatingCashFlow,

            // الأنشطة الاستثمارية
            CapitalExpenditures = -capitalExpenditures,
            AssetDisposals = assetDisposals,
            InvestingCashFlow = investingCashFlow,

            // الأنشطة التمويلية
            LoanProceeds = loanProceeds,
            LoanRepayments = -loanRepayments,
            DividendsPaid = -dividendsPaid,
            FinancingCashFlow = financingCashFlow,

            NetCashChange = netCashChange
        };
    }

    private async Task<decimal> GetNetIncomeAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        var revenue = await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.PostingDate >= period.StartDate && e.PostingDate <= period.EndDate
                     && e.AccountType == AccountType.Revenue)
            .SumAsync(e => e.CreditAmount - e.DebitAmount, ct);

        var expenses = await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.PostingDate >= period.StartDate && e.PostingDate <= period.EndDate
                     && e.AccountType == AccountType.Expense)
            .SumAsync(e => e.DebitAmount - e.CreditAmount, ct);

        return revenue - expenses;
    }

    private async Task<decimal> GetDepreciationAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        return await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.PostingDate >= period.StartDate && e.PostingDate <= period.EndDate
                     && e.AccountCategory == AccountCategory.DepreciationExpense)
            .SumAsync(e => e.DebitAmount - e.CreditAmount, ct);
    }

    private async Task<decimal> GetAmortizationAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        return await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.PostingDate >= period.StartDate && e.PostingDate <= period.EndDate
                     && e.AccountCategory == AccountCategory.AmortizationExpense)
            .SumAsync(e => e.DebitAmount - e.CreditAmount, ct);
    }

    private async Task<decimal> GetARChangeAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        // زيادة AR = تدفق خارج (استهلاك نقدية) — رقم موجب يعني زيادة
        var arStart = await GetAccountBalanceAsync(companyId, AccountCategory.AccountsReceivable, period.StartDate.AddDays(-1), ct);
        var arEnd = await GetAccountBalanceAsync(companyId, AccountCategory.AccountsReceivable, period.EndDate, ct);
        return arEnd - arStart;
    }

    private async Task<decimal> GetInventoryChangeAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        var invStart = await GetAccountBalanceAsync(companyId, AccountCategory.Inventory, period.StartDate.AddDays(-1), ct);
        var invEnd = await GetAccountBalanceAsync(companyId, AccountCategory.Inventory, period.EndDate, ct);
        return invEnd - invStart;
    }

    private async Task<decimal> GetAPChangeAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        // زيادة AP = تدفق داخل (توفير نقدية) — رقم موجب يعني زيادة
        var apStart = await GetAccountBalanceAsync(companyId, AccountCategory.AccountsPayable, period.StartDate.AddDays(-1), ct);
        var apEnd = await GetAccountBalanceAsync(companyId, AccountCategory.AccountsPayable, period.EndDate, ct);
        return apEnd - apStart;
    }

    private async Task<decimal> GetOtherWorkingCapitalChangeAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
        => 0m; // تُحدَّث في Sprint 3 بمزيد من التفاصيل

    private async Task<decimal> GetCapExAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        return await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.PostingDate >= period.StartDate && e.PostingDate <= period.EndDate
                     && e.AccountCategory == AccountCategory.FixedAsset
                     && e.SourceModule == "FixedAssets")
            .SumAsync(e => e.DebitAmount - e.CreditAmount, ct);
    }

    private async Task<decimal> GetAssetDisposalsAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
        => 0m; // تُحدَّث لاحقاً

    private async Task<decimal> GetLoanProceedsAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
        => 0m;

    private async Task<decimal> GetLoanRepaymentsAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
        => 0m;

    private async Task<decimal> GetDividendsPaidAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
        => 0m;

    private async Task<decimal> GetAccountBalanceAsync(Guid companyId, AccountCategory category, DateTime asOfDate, CancellationToken ct)
    {
        return await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.AccountCategory == category
                     && e.PostingDate <= asOfDate)
            .SumAsync(e => e.DebitAmount - e.CreditAmount, ct);
    }
}

// ─── DTOs ─────────────────────────────────────────────────────────

public class BalanceSheetDto
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
    public DateTime AsOfDate { get; init; }
    public List<BalanceSheetSection> Assets { get; init; } = new();
    public List<BalanceSheetSection> Liabilities { get; init; } = new();
    public List<BalanceSheetSection> Equity { get; init; } = new();
    public decimal TotalAssets { get; init; }
    public decimal TotalLiabilities { get; init; }
    public decimal TotalEquity { get; init; }
    public bool IsBalanced { get; init; }
    public decimal BalanceDifference { get; init; }
}

public class BalanceSheetSection
{
    public AccountCategory Category { get; init; }
    public List<BalanceSheetLine> Lines { get; init; } = new();
    public decimal Total { get; init; }
}

public class BalanceSheetLine
{
    public string AccountCode { get; init; } = default!;
    public string AccountName { get; init; } = default!;
    public decimal Amount { get; init; }
}

public class CashFlowDto
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
    public string PeriodName { get; init; } = default!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    // Operating
    public decimal NetIncome { get; init; }
    public decimal Depreciation { get; init; }
    public decimal Amortization { get; init; }
    public decimal ARIncrease { get; init; }
    public decimal InventoryIncrease { get; init; }
    public decimal APIncrease { get; init; }
    public decimal OtherWorkingCapitalChange { get; init; }
    public decimal OperatingCashFlow { get; init; }

    // Investing
    public decimal CapitalExpenditures { get; init; }
    public decimal AssetDisposals { get; init; }
    public decimal InvestingCashFlow { get; init; }

    // Financing
    public decimal LoanProceeds { get; init; }
    public decimal LoanRepayments { get; init; }
    public decimal DividendsPaid { get; init; }
    public decimal FinancingCashFlow { get; init; }

    public decimal NetCashChange { get; init; }
}

public class AccountBalance
{
    public string AccountCode { get; init; } = default!;
    public string AccountName { get; init; } = default!;
    public AccountType AccountType { get; init; }
    public AccountCategory AccountCategory { get; init; }
    public decimal DebitTotal { get; init; }
    public decimal CreditTotal { get; init; }
    public decimal NetBalance { get; init; }
}
