using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Events.Manufacturing;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.Domain.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Manufacturing;


public class ProductionOrder : AuditableEntity, IAggregateRoot, ICompanyEntity
{
    public Guid CompanyId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    
    public Guid BillOfMaterialsId { get; set; }
    public BillOfMaterials BillOfMaterials { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public Item Product { get; set; } = null!;
    
    public decimal Quantity { get; set; } // RULE-MFG15: Production Quantity
    public decimal ProducedQuantity { get; set; }
    
    public ProductionOrderStatus Status { get; set; } // RULE-MFG16: Status (Planned/InProgress/Completed/Cancelled)
    
    public DateTime PlannedStartDate { get; set; } // RULE-MFG17: Planned Start Date
    public DateTime PlannedEndDate { get; set; } // RULE-MFG18: Planned End Date
    public DateTime? ActualStartDate { get; set; } // RULE-MFG19: Actual Start Date
    public DateTime? ActualEndDate { get; set; } // RULE-MFG20: Actual End Date
    
    public decimal PlannedMaterialCost { get; set; }
    public decimal PlannedLaborCost { get; set; }
    public decimal PlannedTotalCost => PlannedMaterialCost + PlannedLaborCost;
    
    public decimal TotalMaterialCost { get; set; }
    public decimal TotalLaborCost { get; set; }
    public decimal TotalCost => TotalMaterialCost + TotalLaborCost;
    
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public virtual ICollection<ProductionOrderMaterial> Materials { get; set; } = new List<ProductionOrderMaterial>();
    public virtual ICollection<ProductionOrderStage> Stages { get; set; } = new List<ProductionOrderStage>();

    // Domain Methods
    public void Start(string updatedBy)
    {
        if (Status != ProductionOrderStatus.Draft && Status != ProductionOrderStatus.Planned)
            throw new ManufacturingDomainException("Cannot start an order that is not in Draft or Planned status.");

        Status = ProductionOrderStatus.InProgress;
        ActualStartDate = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        // Auto-start first stage if applicable
        var firstStage = Stages.OrderBy(s => s.Sequence).FirstOrDefault();
        if (firstStage != null)
        {
            firstStage.Status = ProductionStageStatus.InProgress;
            firstStage.StartDate = DateTime.UtcNow;
        }
    }

    public void StartStage(Guid stageId, string updatedBy)
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new ManufacturingDomainException("Cannot start a stage for an order that is not in progress.");

        var stage = Stages.FirstOrDefault(s => s.Id == stageId);
        if (stage == null)
            throw new ManufacturingDomainException("Stage not found.");

        if (stage.Status != ProductionStageStatus.Pending)
            throw new ManufacturingDomainException("Only pending stages can be started.");

        stage.Status = ProductionStageStatus.InProgress;
        stage.StartDate = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteStage(Guid stageId, decimal actualHours, string updatedBy)
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new ManufacturingDomainException("Cannot complete a stage for an order that is not in progress.");

        var stage = Stages.FirstOrDefault(s => s.Id == stageId);
        if (stage == null)
            throw new ManufacturingDomainException("Stage not found.");

        if (stage.Status != ProductionStageStatus.InProgress)
            throw new ManufacturingDomainException("Only in-progress stages can be completed.");

        stage.Status = ProductionStageStatus.Completed;
        stage.ActualHours = actualHours;
        stage.EndDate = DateTime.UtcNow;
        
        TotalLaborCost += actualHours * stage.CostPerHour;
        
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        // Auto-start next stage
        var nextStage = Stages.Where(s => s.Sequence > stage.Sequence)
                             .OrderBy(s => s.Sequence)
                             .FirstOrDefault();
        if (nextStage != null && nextStage.Status == ProductionStageStatus.Pending)
        {
            nextStage.Status = ProductionStageStatus.InProgress;
            nextStage.StartDate = DateTime.UtcNow;
        }
    }

    public void ConsumeMaterial(Guid materialId, decimal quantity, string updatedBy)
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new ManufacturingDomainException("Cannot consume material for an order that is not in progress.");

        if (quantity <= 0)
            throw new ManufacturingDomainException("Consumed quantity must be greater than zero.");

        var material = Materials.FirstOrDefault(m => m.RawMaterialId == materialId);
        if (material == null)
            throw new ManufacturingDomainException("Material not found in this production order.");

        material.ActualQuantity += quantity;
        TotalMaterialCost += quantity * material.UnitCost;
        
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new MaterialConsumedEvent(Id, materialId, quantity, material.UnitCost, WarehouseId));
    }

    public void Complete(decimal producedQty, decimal totalLaborCost, decimal totalMaterialCost, Guid warehouseId, string updatedBy)
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new ManufacturingDomainException("Cannot complete an order that is not in progress.");
            
        if (producedQty <= 0)
            throw new ManufacturingDomainException("Produced quantity must be greater than zero.");
            
        if (warehouseId == Guid.Empty)
            throw new ManufacturingDomainException("Warehouse ID is required to complete production order.");

        Status = ProductionOrderStatus.Completed;
        ProducedQuantity = producedQty;
        TotalLaborCost = totalLaborCost;
        TotalMaterialCost = totalMaterialCost;
        WarehouseId = warehouseId;
        ActualEndDate = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProductionOrderCompletedEvent(Id, ProductId, producedQty, TotalCost, TotalMaterialCost, TotalLaborCost, warehouseId));
    }

    public void Cancel(string updatedBy)
    {
        if (Status == ProductionOrderStatus.Completed)
            throw new ManufacturingDomainException("Cannot cancel a completed production order.");

        Status = ProductionOrderStatus.Cancelled;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
