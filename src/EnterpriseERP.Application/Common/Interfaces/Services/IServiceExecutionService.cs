using EnterpriseERP.Domain.Entities.Beauty;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

/// <summary>
/// Service Execution Engine.
/// Converts a salon service appointment into:
/// 1. Actual inventory deductions (per recipe)
/// 2. Profitability calculation per service
/// 3. Staff commission trigger
/// KEY DIFFERENTIATOR: Real-time material consumption tracking.
/// </summary>
public interface IServiceExecutionService
{
    /// <summary>
    /// Start execution of a service — creates ServiceExecution record.
    /// </summary>
    Task<ServiceExecution> StartServiceAsync(
        Guid appointmentId,
        Guid appointmentLineId,
        string staffUserId,
        string staffName,
        DateTime startTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete service execution:
    /// 1. Deduct materials from inventory (FEFO — First Expiry First Out)
    /// 2. Calculate actual material cost
    /// 3. Calculate gross profit
    /// 4. Trigger staff commission
    /// </summary>
    Task<ServiceExecutionResult> CompleteServiceAsync(
        Guid serviceExecutionId,
        DateTime endTime,
        List<MaterialConsumptionInput>? overrides = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get profitability report for a service/period.
    /// </summary>
    Task<ServiceProfitabilityDto> GetProfitabilityAsync(
        Guid companyId,
        Guid? serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}

public record MaterialConsumptionInput(
    Guid BeautyProductId,
    decimal ActualQuantity,
    string Unit
);

public record ServiceExecutionResult(
    Guid ServiceExecutionId,
    decimal ServicePrice,
    decimal TotalMaterialCost,
    decimal GrossProfit,
    decimal GrossProfitPercent,
    List<MaterialConsumptionDetail> MaterialsConsumed
);

public record MaterialConsumptionDetail(
    string ProductName,
    decimal PlannedQuantity,
    decimal ActualQuantity,
    decimal VarianceQuantity,
    decimal UnitCost,
    decimal TotalCost
);

public record ServiceProfitabilityDto(
    Guid? ServiceId,
    string ServiceName,
    int ExecutionCount,
    decimal TotalRevenue,
    decimal TotalMaterialCost,
    decimal TotalGrossProfit,
    decimal AvgGrossProfitPercent
);
