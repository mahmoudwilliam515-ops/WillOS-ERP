using System;
using System.Collections.Generic;
using EnterpriseERP.Application.Features.Manufacturing.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CreateBillOfMaterials;

public class CreateBillOfMaterialsCommand : IRequest<BillOfMaterialsDto>
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public bool IsDefault { get; set; }
    public decimal LaborCostPerHour { get; set; }
    public decimal MachineOverheadPerHour { get; set; }
    
    public List<CreateBillOfMaterialsLineRequest> Lines { get; set; } = new();
    public List<CreateProductionStageRequest> Stages { get; set; } = new();
}

public class CreateBillOfMaterialsLineRequest
{
    public Guid MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreateProductionStageRequest
{
    public string Name { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal CostPerHour { get; set; }
    public Guid? WorkCenterId { get; set; }
    public string Description { get; set; } = string.Empty;
}
