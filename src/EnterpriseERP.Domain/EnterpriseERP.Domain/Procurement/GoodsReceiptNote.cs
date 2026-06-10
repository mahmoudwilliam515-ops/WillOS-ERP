using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Procurement.Events;

namespace EnterpriseERP.Domain.Procurement;

/// <summary>
/// سند استلام البضاعة (GRN) — الحلقة الثانية في دورة P2P
/// Blueprint Section 1.1 — Goods Receipt Note
/// </summary>
public class GoodsReceiptNote : AuditableEntity
{
    public Guid Id { get; private set; }
    public string GRNNumber { get; private set; } = default!;
    public Guid PurchaseOrderId { get; private set; }
    public Guid CompanyId { get; private set; }      // TenantId — دائماً مطلوب
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTime ReceiptDate { get; private set; }
    public GRNStatus Status { get; private set; }
    public string? DeliveryNoteReference { get; private set; }  // رقم إشعار التسليم من المورد
    public string? Notes { get; private set; }

    private readonly List<GoodsReceiptNoteLine> _lines = new();
    public IReadOnlyCollection<GoodsReceiptNoteLine> Lines => _lines.AsReadOnly();

    // EF Core constructor
    private GoodsReceiptNote() { }

    public static GoodsReceiptNote Create(
        Guid purchaseOrderId,
        Guid companyId,
        Guid supplierId,
        Guid warehouseId,
        DateTime receiptDate,
        string grnNumber,
        string? deliveryNoteReference = null,
        string? notes = null)
    {
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("PurchaseOrderId is required", nameof(purchaseOrderId));
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required", nameof(companyId));

        return new GoodsReceiptNote
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrderId,
            CompanyId = companyId,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ReceiptDate = receiptDate,
            GRNNumber = grnNumber,
            DeliveryNoteReference = deliveryNoteReference,
            Notes = notes,
            Status = GRNStatus.Draft
        };
    }

    public void AddLine(
        Guid purchaseOrderLineId,
        Guid itemId,
        decimal receivedQuantity,
        decimal unitCost,
        string? batchNumber = null)
    {
        if (Status != GRNStatus.Draft)
            throw new InvalidOperationException($"Cannot add lines to GRN in status {Status}. Only Draft GRNs can be modified.");

        if (receivedQuantity <= 0)
            throw new ArgumentException("Received quantity must be positive", nameof(receivedQuantity));

        if (unitCost < 0)
            throw new ArgumentException("Unit cost cannot be negative", nameof(unitCost));

        _lines.Add(GoodsReceiptNoteLine.Create(Id, purchaseOrderLineId, itemId, receivedQuantity, unitCost, batchNumber));
    }

    /// <summary>
    /// اعتماد الاستلام — يُطلق Domain Event لتشغيل GRNI Accrual Posting
    /// Dr: GRNI Accrual Account / Cr: AP Accrued Liability
    /// </summary>
    public void Approve(Guid approvedByUserId)
    {
        if (Status != GRNStatus.Draft)
            throw new InvalidOperationException($"Cannot approve GRN in status {Status}. Only Draft GRNs can be approved.");

        if (!_lines.Any())
            throw new InvalidOperationException("Cannot approve GRN with no lines.");

        Status = GRNStatus.Approved;
        AddDomainEvent(new GRNApprovedEvent(Id, CompanyId, PurchaseOrderId, approvedByUserId, ReceiptDate, _lines.ToList()));
    }

    public void Cancel(string reason)
    {
        if (Status == GRNStatus.Approved)
            throw new InvalidOperationException("Cannot cancel an approved GRN. Create a Purchase Return instead.");

        Status = GRNStatus.Cancelled;
    }
}

public class GoodsReceiptNoteLine : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid GoodsReceiptNoteId { get; private set; }
    public Guid PurchaseOrderLineId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost => ReceivedQuantity * UnitCost;
    public string? BatchNumber { get; private set; }

    // حالة المطابقة — يُحدَّث بواسطة ThreeWayMatchService
    public LineMatchStatus MatchStatus { get; private set; } = LineMatchStatus.Pending;

    // Navigation property
    public GoodsReceiptNote GoodsReceiptNote { get; private set; } = default!;

    private GoodsReceiptNoteLine() { }

    internal static GoodsReceiptNoteLine Create(
        Guid grnId,
        Guid purchaseOrderLineId,
        Guid itemId,
        decimal receivedQuantity,
        decimal unitCost,
        string? batchNumber)
    {
        return new GoodsReceiptNoteLine
        {
            Id = Guid.NewGuid(),
            GoodsReceiptNoteId = grnId,
            PurchaseOrderLineId = purchaseOrderLineId,
            ItemId = itemId,
            ReceivedQuantity = receivedQuantity,
            UnitCost = unitCost,
            BatchNumber = batchNumber
        };
    }

    public void SetMatchStatus(LineMatchStatus status) => MatchStatus = status;
}

public enum GRNStatus
{
    Draft = 0,
    Approved = 1,
    Cancelled = 2
}

public enum LineMatchStatus
{
    Pending = 0,
    Matched = 1,
    PartialMatch = 2,
    Variance = 3
}
