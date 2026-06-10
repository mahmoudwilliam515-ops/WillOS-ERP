using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public record PredictionData(DateTime Date, decimal PredictedAmount);
public record InventoryPrediction(Guid ItemId, string ItemName, decimal PredictedUsagePerDay, int DaysUntilStockOut, decimal CurrentStock);

public interface IAIPredictiveService
{
    Task<IEnumerable<PredictionData>> PredictCashFlowAsync(int daysAhead);
    Task<IEnumerable<InventoryPrediction>> PredictInventoryReplenishmentAsync();
}
