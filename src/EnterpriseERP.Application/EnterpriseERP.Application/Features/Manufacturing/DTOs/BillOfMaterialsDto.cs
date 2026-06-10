using System;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.Manufacturing.DTOs;

public class BillOfMaterialsDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public decimal LaborCostPerHour { get; set; }
    public decimal MachineOverheadPerHour { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public List<BillOfMaterialsLineDto> Lines { get; set; } = new();
    public List<ProductionStageDto> Stages { get; set; } = new();
}

public class BillOfMaterialsLineDto
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal ScrapFactor { get; set; }
    public decimal LineTotal => Quantity * (1 + ScrapFactor);
}

public class ProductionStageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal CostPerHour { get; set; }
    public Guid? WorkCenterId { get; set; }
    public string WorkCenter { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
