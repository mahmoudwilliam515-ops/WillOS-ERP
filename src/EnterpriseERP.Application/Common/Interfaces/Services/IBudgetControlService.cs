using EnterpriseERP.Domain.Entities.Accounting;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

/// <summary>
/// Budget Control Engine Interface.
/// RULE-BUDGET01: Every expenditure must pass budget check before commitment.
/// </summary>
public interface IBudgetControlService
{
    /// <summary>
    /// Check if the requested amount is within budget for a given account/period.
    /// </summary>
    Task<BudgetControlResult> CheckBudgetAsync(
        Guid companyId,
        Guid accountId,
        Guid fiscalYearId,
        int month,
        decimal requestedAmount,
        Guid? costCenterId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create an encumbrance (pre-commitment) against budget.
    /// Called when PO or Requisition is approved.
    /// </summary>
    Task<Encumbrance> EncumberAsync(
        Guid companyId,
        Guid accountId,
        Guid fiscalYearId,
        int month,
        decimal amount,
        EncumbranceType sourceType,
        Guid sourceDocumentId,
        string sourceDocumentNumber,
        Guid? costCenterId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Release encumbrance when invoice is posted against PO.
    /// </summary>
    Task ReleaseEncumbranceAsync(
        Guid encumbranceId,
        decimal releaseAmount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get budget utilization summary for a given account/period.
    /// </summary>
    Task<BudgetUtilizationDto> GetUtilizationAsync(
        Guid companyId,
        Guid accountId,
        Guid fiscalYearId,
        int month,
        Guid? costCenterId = null,
        CancellationToken cancellationToken = default);
}

public record BudgetUtilizationDto(
    decimal TotalBudget,
    decimal EncumberedAmount,
    decimal ActualSpent,
    decimal AvailableBalance,
    decimal UtilizationPercent
);
