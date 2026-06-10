using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Inventory;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Purchasing;

public enum PurchaseRequisitionStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    ConvertedToPO = 4,
    Cancelled = 5
}

public class PurchaseRequisition : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string RequisitionNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public PurchaseRequisitionStatus Status { get; set; } = PurchaseRequisitionStatus.Draft;
    public Guid RequestedById { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid BranchId { get; set; }

    public ICollection<PurchaseRequisitionLine> Lines { get; set; } = new List<PurchaseRequisitionLine>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class PurchaseRequisitionLine : BaseEntity
{
    public Guid PurchaseRequisitionId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal EstimatedUnitCost { get; set; }
    public decimal LineTotal { get; set; }
    public string Notes { get; set; } = string.Empty;

    public PurchaseRequisition PurchaseRequisition { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
