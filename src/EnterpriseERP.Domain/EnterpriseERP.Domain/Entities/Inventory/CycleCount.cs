using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Inventory;

/// <summary>
/// Cycle Count: a scheduled physical count of a specific item or warehouse section.
/// Used for periodic inventory accuracy verification without full warehouse shutdown.
/// </summary>
public class CycleCount : AuditableEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public string CountNumber { get; set; } = string.Empty;
    public DateTime CountDate { get; set; }
    public CycleCountStatus Status { get; set; } = CycleCountStatus.Planned;
    public string? Notes { get; set; }

    public ICollection<CycleCountLine> Lines { get; set; } = new List<CycleCountLine>();
}

public enum CycleCountStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Approved = 3
}

public class CycleCountLine : AuditableEntity
{
    public Guid CycleCountId { get; set; }
    public CycleCount CycleCount { get; set; } = null!;

    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public decimal SystemQuantity { get; set; }  // Quantity according to system records
    public decimal CountedQuantity { get; set; } // Quantity physically counted
    public decimal Variance => CountedQuantity - SystemQuantity;

    public bool IsAdjusted { get; set; } = false;
    public string? Notes { get; set; }
}
