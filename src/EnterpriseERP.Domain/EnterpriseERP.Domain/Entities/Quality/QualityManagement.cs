using EnterpriseERP.SharedKernel.Common;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Quality;

public enum InspectionType
{
    PurchaseReceipt = 0,
    ProductionOutput = 1,
    InventoryTransfer = 2,
    SalesReturn = 3
}

public class QualityChecklist : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InspectionType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public List<QualityChecklistItem> Items { get; set; } = new();
}

public class QualityChecklistItem : BaseEntity
{
    public string Requirement { get; set; } = string.Empty;
    public bool IsMandatory { get; set; } = true;
    public int Sequence { get; set; }
}

public enum InspectionStatus
{
    Pending = 0,
    Passed = 1,
    Failed = 2,
    ConditionalPass = 3
}

public class QualityInspection : AuditableEntity, IAggregateRoot
{
    public string InspectionNumber { get; set; } = string.Empty;
    public Guid ChecklistId { get; set; }
    public QualityChecklist Checklist { get; set; } = null!;

    public InspectionType Type { get; set; }
    public Guid ReferenceId { get; set; } // GRN ID, Production Order ID, etc.
    public string ReferenceNumber { get; set; } = string.Empty;

    public InspectionStatus Status { get; set; } = InspectionStatus.Pending;
    public string InspectorName { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }
    
    public string Notes { get; set; } = string.Empty;
    public List<InspectionResultItem> Results { get; set; } = new();
}

public class InspectionResultItem : BaseEntity
{
    public Guid ChecklistItemId { get; set; }
    public string Requirement { get; set; } = string.Empty;
    public bool IsPassed { get; set; }
    public string Finding { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
}
