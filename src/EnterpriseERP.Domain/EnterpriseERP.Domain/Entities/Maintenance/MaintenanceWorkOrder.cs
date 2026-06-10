using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.FixedAssets;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Maintenance;

public class MaintenanceWorkOrder : AuditableEntity, IAggregateRoot
{
    public string OrderNumber { get; set; } = string.Empty;
    
    public Guid AssetId { get; set; }
    public FixedAsset Asset { get; set; } = null!;
    
    public Guid? AssignedTechnicianId { get; set; }
    public MaintenanceTechnician? AssignedTechnician { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
    
    public MaintenanceWorkOrderStatus Status { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    
    public decimal TotalPartsCost { get; set; }
    public decimal TotalLaborCost { get; set; }
    public decimal TotalCost => TotalPartsCost + TotalLaborCost;
    
    public ICollection<MaintenanceWorkOrderPart> Parts { get; set; } = new List<MaintenanceWorkOrderPart>();

    // Domain Methods
    public void AssignTechnician(Guid technicianId, string updatedBy)
    {
        if (Status == MaintenanceWorkOrderStatus.Completed || Status == MaintenanceWorkOrderStatus.Cancelled)
            throw new Exceptions.MaintenanceDomainException("Cannot assign technician to a completed or cancelled work order.");
            
        AssignedTechnicianId = technicianId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartWork(string updatedBy)
    {
        if (Status != MaintenanceWorkOrderStatus.Open && Status != MaintenanceWorkOrderStatus.WaitingForParts)
            throw new Exceptions.MaintenanceDomainException("Cannot start work from the current status.");
            
        if (AssignedTechnicianId == null)
            throw new Exceptions.MaintenanceDomainException("Cannot start work without an assigned technician.");

        Status = MaintenanceWorkOrderStatus.InProgress;
        StartDate = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new Events.Maintenance.MaintenanceWorkOrderStartedEvent(Id, AssetId));
    }

    public void Complete(string resolutionNotes, decimal totalPartsCost, decimal totalLaborCost, string updatedBy)
    {
        if (Status != MaintenanceWorkOrderStatus.InProgress)
            throw new Exceptions.MaintenanceDomainException("Work order must be in progress to be completed.");

        Status = MaintenanceWorkOrderStatus.Completed;
        ResolutionNotes = resolutionNotes;
        TotalPartsCost = totalPartsCost;
        TotalLaborCost = totalLaborCost;
        CompletionDate = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new Events.Maintenance.MaintenanceWorkOrderCompletedEvent(Id, AssetId, TotalCost));
    }

    public void Cancel(string updatedBy)
    {
        if (Status == MaintenanceWorkOrderStatus.Completed)
            throw new Exceptions.MaintenanceDomainException("Cannot cancel a completed work order.");

        Status = MaintenanceWorkOrderStatus.Cancelled;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
