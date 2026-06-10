using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Accounting.Budgets.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.Budgets.Queries.GetBudgetVsActual;

public class GetBudgetVsActualQueryHandler : IRequestHandler<GetBudgetVsActualQuery, Result<BudgetVsActualDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBudgetVsActualQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BudgetVsActualDto>> Handle(GetBudgetVsActualQuery request, CancellationToken cancellationToken)
    {
        var budget = await _unitOfWork.Repository<Budget>().GetByIdAsync(request.BudgetId);
        if (budget == null)
            return Result.Failure<BudgetVsActualDto>(new Error("BudgetVsActual.NotFound", "Budget not found."));

        // Load fiscal year to get date range for actual amounts
        var fiscalYear = await _unitOfWork.Repository<FiscalYear>().GetByIdAsync(budget.FiscalYearId);
        if (fiscalYear == null)
            return Result.Failure<BudgetVsActualDto>(new Error("BudgetVsActual.FiscalYearNotFound", "Fiscal year not found."));

        // Get all journal entry lines for the fiscal year date range
        var journalEntryLines = await _unitOfWork.Repository<JournalEntryLine>()
            .FindAsync(jel => jel.JournalEntry != null &&
                              jel.JournalEntry.EntryDate >= fiscalYear.StartDate &&
                              jel.JournalEntry.EntryDate <= fiscalYear.EndDate &&
                              jel.JournalEntry.Status == JournalEntryStatus.Posted);

        // Group actuals by account code
        var actualsByAccount = journalEntryLines
            .GroupBy(jel => jel.AccountCode)
            .ToDictionary(g => g.Key, g => g.Sum(jel => jel.DebitAmount - jel.CreditAmount));

        var resultLines = new List<BudgetVsActualLineDto>();

        foreach (var line in budget.Lines)
        {
            var account = await _unitOfWork.Repository<Account>().GetByIdAsync(line.AccountId);
            if (account == null) continue;

            var actualAmount = actualsByAccount.TryGetValue(account.Code, out var actual) ? actual : 0m;
            var variance = line.TotalPlanned - actualAmount;
            var variancePct = line.TotalPlanned != 0
                ? Math.Round(variance / line.TotalPlanned * 100, 2)
                : 0;

            resultLines.Add(new BudgetVsActualLineDto
            {
                AccountCode = account.Code,
                AccountName = account.Name,
                PlannedAmount = line.TotalPlanned,
                ActualAmount = actualAmount,
                Variance = variance,
                VariancePercent = variancePct
            });
        }

        var totalPlanned = resultLines.Sum(l => l.PlannedAmount);
        var totalActual = resultLines.Sum(l => l.ActualAmount);
        var totalVariance = totalPlanned - totalActual;

        return Result.Success(new BudgetVsActualDto
        {
            BudgetId = budget.Id,
            BudgetName = budget.Name,
            FiscalYearName = fiscalYear.Name,
            Lines = resultLines,
            TotalPlanned = totalPlanned,
            TotalActual = totalActual,
            TotalVariance = totalVariance,
            VariancePercent = totalPlanned != 0
                ? Math.Round(totalVariance / totalPlanned * 100, 2)
                : 0
        });
    }
}
