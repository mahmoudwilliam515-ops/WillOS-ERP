using System;

namespace EnterpriseERP.Application.Features.Manufacturing.DTOs;

public class ProductionProfitabilityDto
{
    public Guid ProductionOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime CompletionDate { get; set; }

    public decimal ActualMaterialCost { get; set; }
    public decimal ActualLaborCost { get; set; }
    public decimal TotalActualCost => ActualMaterialCost + ActualLaborCost;

    public decimal PlannedTotalCost { get; set; }
    public decimal CostVariance => TotalActualCost - PlannedTotalCost;
    public decimal CostVariancePercentage => PlannedTotalCost != 0 ? (CostVariance / PlannedTotalCost) * 100 : 0;

    public decimal EstimatedRevenue { get; set; }
    public decimal GrossProfit => EstimatedRevenue - TotalActualCost;
    public decimal MarginPercentage => EstimatedRevenue != 0 ? (GrossProfit / EstimatedRevenue) * 100 : 0;
}
