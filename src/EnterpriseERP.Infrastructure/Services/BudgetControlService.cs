using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

/// <summary>
/// Budget Control Engine Implementation.
/// RULE-BUDGET01: Enforces budget limits across all expenditure transactions.
/// RULE-BUDGET02: Encumbrance reduces available budget before actual spend.
/// </summary>
public class BudgetControlService : IBudgetControlService
{
    private readonly ApplicationDbContext _context;

    // Tolerance: allow up to 5% overrun before hard block (configurable)
    private const decimal OverrunTolerancePercent = 0.05m;

    public BudgetControlService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BudgetControlResult> CheckBudgetAsync(
        Guid companyId,
        Guid accountId,
        Guid fiscalYearId,
        int month,
        decimal requestedAmount,
        Guid? costCenterId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Find approved budget line
        var budgetLine = await GetBudgetLineAsync(companyId, accountId, fiscalYearId, costCenterId, cancellationToken);

        if (budgetLine == null)
            return BudgetControlResult.Blocked(BudgetControlViolationType.NoBudgetFound, requestedAmount, 0, 0, 0);

        if (budgetLine.Budget.Status != BudgetStatus.Approved)
            return BudgetControlResult.Blocked(BudgetControlViolationType.BudgetNotApproved, requestedAmount, 0, 0, 0);

        // 2. Get planned amount for the month
        var plannedAmount = GetMonthAmount(budgetLine, month);

        // 2.5. Get fiscal year for date comparison
        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(f => f.Id == fiscalYearId, cancellationToken)
            ?? throw new InvalidOperationException($"FiscalYear {fiscalYearId} not found.");

        // 3. Get active encumbrances for this account/period
        var encumbered = await _context.Set<Encumbrance>()
            .Where(e => e.AccountId == accountId
                     && e.BudgetId == budgetLine.BudgetId
                     && e.Status == EncumbranceStatus.Active
                     && e.EncumbranceDate.Month == month)
            .SumAsync(e => e.RemainingAmount, cancellationToken);

        // 4. Get actual spent (journal entries posted this month)
        var actualSpent = await _context.JournalEntryLines
            .Where(l => l.AccountCode == budgetLine.Account.Code
                     && l.JournalEntry.EntryDate.Year == fiscalYear.Year
                     && l.JournalEntry.EntryDate.Month == month
                     && l.JournalEntry.Status == JournalEntryStatus.Posted)
            .SumAsync(l => l.DebitAmount, cancellationToken);

        // 5. Available = Planned - Encumbered - Actual
        var available = plannedAmount * (1 + OverrunTolerancePercent);
        var consumed  = encumbered + actualSpent + requestedAmount;

        if (consumed > available)
            return BudgetControlResult.Blocked(
                BudgetControlViolationType.ExceedsBudget,
                requestedAmount, plannedAmount, encumbered, actualSpent);

        return BudgetControlResult.Allowed(requestedAmount, plannedAmount, encumbered, actualSpent);
    }

    public async Task<Encumbrance> EncumberAsync(
        Guid companyId,
        Guid accountId,
        Guid fiscalYearId,
        int month,
        decimal amount,
        EncumbranceType sourceType,
        Guid sourceDocumentId,
        string sourceDocumentNumber,
        Guid? costCenterId = null,
        CancellationToken cancellationToken = default)
    {
        // Run budget check first
        var check = await CheckBudgetAsync(companyId, accountId, fiscalYearId, month, amount, costCenterId, cancellationToken);
        if (!check.IsAllowed)
            throw new InvalidOperationException($"Budget control blocked encumbrance: {check.Message}");

        var budgetLine = await GetBudgetLineAsync(companyId, accountId, fiscalYearId, costCenterId, cancellationToken);

        var encumbrance = new Encumbrance
        {
            Id                   = Guid.NewGuid(),
            BudgetId             = budgetLine!.BudgetId,
            AccountId            = accountId,
            CostCenterId         = costCenterId,
            SourceType           = sourceType,
            SourceDocumentId     = sourceDocumentId,
            SourceDocumentNumber = sourceDocumentNumber,
            EncumberedAmount     = amount,
            ReleasedAmount       = 0,
            Status               = EncumbranceStatus.Active,
            EncumbranceDate      = DateTime.UtcNow
        };

        await _context.Set<Encumbrance>().AddAsync(encumbrance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return encumbrance;
    }

    public async Task ReleaseEncumbranceAsync(
        Guid encumbranceId,
        decimal releaseAmount,
        CancellationToken cancellationToken = default)
    {
        var encumbrance = await _context.Set<Encumbrance>()
            .FirstOrDefaultAsync(e => e.Id == encumbranceId, cancellationToken)
            ?? throw new InvalidOperationException($"Encumbrance {encumbranceId} not found.");

        if (encumbrance.Status != EncumbranceStatus.Active)
            throw new InvalidOperationException($"Encumbrance {encumbranceId} is not active.");

        encumbrance.ReleasedAmount += releaseAmount;
        encumbrance.ReleaseDate     = DateTime.UtcNow;

        if (encumbrance.ReleasedAmount >= encumbrance.EncumberedAmount)
            encumbrance.Status = EncumbranceStatus.Released;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BudgetUtilizationDto> GetUtilizationAsync(
        Guid companyId,
        Guid accountId,
        Guid fiscalYearId,
        int month,
        Guid? costCenterId = null,
        CancellationToken cancellationToken = default)
    {
        var budgetLine = await GetBudgetLineAsync(companyId, accountId, fiscalYearId, costCenterId, cancellationToken);
        if (budgetLine == null)
            return new BudgetUtilizationDto(0, 0, 0, 0, 0);

        var planned = GetMonthAmount(budgetLine, month);

        var encumbered = await _context.Set<Encumbrance>()
            .Where(e => e.AccountId == accountId
                     && e.BudgetId == budgetLine.BudgetId
                     && e.Status == EncumbranceStatus.Active
                     && e.EncumbranceDate.Month == month)
            .SumAsync(e => e.RemainingAmount, cancellationToken);

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(f => f.Id == fiscalYearId, cancellationToken)
            ?? throw new InvalidOperationException($"FiscalYear {fiscalYearId} not found.");

        var actual = await _context.JournalEntryLines
            .Where(l => l.AccountCode == budgetLine.Account.Code
                     && l.JournalEntry.EntryDate.Year == fiscalYear.Year
                     && l.JournalEntry.EntryDate.Month == month
                     && l.JournalEntry.Status == JournalEntryStatus.Posted)
            .SumAsync(l => l.DebitAmount, cancellationToken);

        var available   = planned - encumbered - actual;
        var utilization = planned > 0 ? Math.Round((encumbered + actual) / planned * 100, 2) : 0;

        return new BudgetUtilizationDto(planned, encumbered, actual, available, utilization);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<BudgetLine?> GetBudgetLineAsync(
        Guid companyId, Guid accountId, Guid fiscalYearId, Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        return await _context.BudgetLines
            .Include(bl => bl.Budget)
            .Include(bl => bl.Account)
            .Where(bl => bl.AccountId == accountId
                      && bl.Budget.FiscalYearId == fiscalYearId
                      && bl.Budget.CostCenterId == costCenterId)
            .OrderByDescending(bl => bl.Budget.Status == BudgetStatus.Approved)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static decimal GetMonthAmount(BudgetLine line, int month) => month switch
    {
        1  => line.Jan, 2  => line.Feb, 3  => line.Mar,
        4  => line.Apr, 5  => line.May, 6  => line.Jun,
        7  => line.Jul, 8  => line.Aug, 9  => line.Sep,
        10 => line.Oct, 11 => line.Nov, 12 => line.Dec,
        _  => throw new ArgumentOutOfRangeException(nameof(month), "Month must be 1-12.")
    };
}


