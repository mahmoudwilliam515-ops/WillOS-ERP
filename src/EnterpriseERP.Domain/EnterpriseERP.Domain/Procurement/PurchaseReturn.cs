using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Procurement.Events;

namespace EnterpriseERP.Domain.Procurement;

/// <summary>
/// مردود المشتريات — يُنشأ عند إعادة بضاعة للمورد بعد استلامها
/// Blueprint Section 1.1 — Purchase Returns
/// القيد: Dr: AP Accrued Liability / Cr: GRNI Accrual (عكس قيد GRN)
/// </summary>
public class PurchaseReturn : AuditableEntity
{
    public Guid Id { get; private set; }
    public string ReturnNumber { get; private set; } = default!;
    public Guid CompanyId { get; private set; }
    public Guid PurchaseInvoiceId { get; private set; }
    public Guid GoodsReceiptNoteId { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTime ReturnDate { get; private set; }
    public string Reason { get; private set; } = default!;
    public decimal TotalAmount { get; private set; }
    public PurchaseReturnStatus Status { get; private set; }

    private readonly List<PurchaseReturnLine> _lines = new();
    public IReadOnlyCollection<PurchaseReturnLine> Lines => _lines.AsReadOnly();

    // EF Core constructor
    private PurchaseReturn() { }

    public static PurchaseReturn Create(
        Guid companyId,
        Guid goodsReceiptNoteId,
        Guid purchaseOrderId,
        Guid supplierId,
        Guid warehouseId,
        DateTime returnDate,
        string returnNumber,
        string reason)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required", nameof(companyId));
        if (goodsReceiptNoteId == Guid.Empty)
            throw new ArgumentException("GoodsReceiptNoteId is required", nameof(goodsReceiptNoteId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Return reason is required", nameof(reason));

        return new PurchaseReturn
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            GoodsReceiptNoteId = goodsReceiptNoteId,
            PurchaseOrderId = purchaseOrderId,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            ReturnDate = returnDate,
            ReturnNumber = returnNumber,
            Reason = reason,
            Status = PurchaseReturnStatus.Draft
        };
    }

    public void AddLine(
        Guid grnLineId,
        Guid itemId,
        decimal returnQuantity,
        decimal unitCost)
    {
        if (Status != PurchaseReturnStatus.Draft)
            throw new InvalidOperationException($"Cannot add lines to PurchaseReturn in status {Status}.");

        if (returnQuantity <= 0)
            throw new ArgumentException("Return quantity must be positive.", nameof(returnQuantity));

        _lines.Add(PurchaseReturnLine.Create(Id, grnLineId, itemId, returnQuantity, unitCost));
    }

    /// <summary>
    /// اعتماد المردود — يُطلق Domain Event لعكس GRNI Posting
    /// Dr: AP Accrued Liability / Cr: GRNI Accrual
    /// </summary>
    public void Approve(Guid approvedByUserId)
    {
        if (Status != PurchaseReturnStatus.Draft)
            throw new InvalidOperationException($"Cannot approve PurchaseReturn in status {Status}.");

        if (!_lines.Any())
            throw new InvalidOperationException("Cannot approve PurchaseReturn with no lines.");

        Status = PurchaseReturnStatus.Approved;
        TotalAmount = _lines.Sum(l => l.ReturnedQuantity * l.UnitCost);
        AddDomainEvent(new PurchaseReturnApprovedEvent(
            Id, CompanyId, GoodsReceiptNoteId, PurchaseOrderId, SupplierId,
            approvedByUserId, ReturnDate, _lines.ToList()));
    }

    public decimal TotalReturnValue => _lines.Sum(l => l.TotalCost);
}

public class PurchaseReturnLine : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid PurchaseReturnId { get; private set; }
    public Guid GRNLineId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal ReturnQuantity { get; private set; }
    public decimal ReturnedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost => ReturnQuantity * UnitCost;

    private PurchaseReturnLine() { }

    internal static PurchaseReturnLine Create(
        Guid purchaseReturnId,
        Guid grnLineId,
        Guid itemId,
        decimal returnQuantity,
        decimal unitCost)
    {
        return new PurchaseReturnLine
        {
            Id = Guid.NewGuid(),
            PurchaseReturnId = purchaseReturnId,
            GRNLineId = grnLineId,
            ItemId = itemId,
            ReturnQuantity = returnQuantity,
            ReturnedQuantity = returnQuantity,
            UnitCost = unitCost
        };
    }
}

public enum PurchaseReturnStatus
{
    Draft = 0,
    Approved = 1,
    Cancelled = 2
}
