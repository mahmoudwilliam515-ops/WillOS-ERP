using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Accounting;

/// <summary>
/// Encumbrance: Pre-commitment of budget funds before actual expenditure.
/// Created when PO/Requisition is approved, released when invoice is posted.
/// RULE-BUDGET01: No expenditure allowed beyond approved budget + tolerance.
/// </summary>
public class Encumbrance : AuditableEntity
{
    public Guid BudgetId { get; set; }
    public Budget Budget { get; set; } = null!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

    // Source document that created this encumbrance
    public EncumbranceType SourceType { get; set; }
    public Guid SourceDocumentId { get; set; }
    public string SourceDocumentNumber { get; set; } = string.Empty;

    public decimal EncumberedAmount { get; set; }
    public decimal ReleasedAmount { get; set; }
    public decimal RemainingAmount => EncumberedAmount - ReleasedAmount;

    public EncumbranceStatus Status { get; set; } = EncumbranceStatus.Active;

    public DateTime EncumbranceDate { get; set; }
    public DateTime? ReleaseDate { get; set; }

    public string? Notes { get; set; }
}

public enum EncumbranceType
{
    PurchaseRequisition = 1,
    PurchaseOrder       = 2,
    TravelRequest       = 3,
    Other               = 99
}

public enum EncumbranceStatus
{
    Active    = 1,
    Released  = 2,
    Cancelled = 3
}
