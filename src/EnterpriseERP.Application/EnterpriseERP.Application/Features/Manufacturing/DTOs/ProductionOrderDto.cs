using System;

namespace EnterpriseERP.Application.Features.Manufacturing.DTOs;

public record ProductionOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid ProductId { get; init; }
    public string? ProductName { get; init; }
    public Guid BillOfMaterialsId { get; init; }
    public decimal PlannedQuantity { get; init; }
    public decimal ProducedQuantity { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal PlannedMaterialCost { get; init; }
    public decimal PlannedLaborCost { get; init; }
    public decimal PlannedTotalCost { get; init; }
    public decimal TotalMaterialCost { get; init; }
    public decimal TotalLaborCost { get; init; }
    public decimal TotalCost { get; init; }
    public DateTime CreatedAt { get; init; }
}
