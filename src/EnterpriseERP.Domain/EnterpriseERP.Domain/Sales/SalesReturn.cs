using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Sales;

/// <summary>
/// مرتجع المبيعات — يُنشئ إشعار دائن ويعكس القيود المحاسبية
/// Blueprint Section 1.2 — Sales Returns
/// </summary>
public class SalesReturn : AuditableEntity
{
    public Guid Id { get; private set; }
    public string ReturnNumber { get; private set; } = default!;
    public Guid CompanyId { get; private set; }
    public Guid OriginalInvoiceId { get; private set; }
    public Guid SalesInvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTime ReturnDate { get; private set; }
    public string ReturnReason { get; private set; } = default!;
    public string Reason { get; private set; } = default!;
    public SalesReturnStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    private readonly List<SalesReturnLine> _lines = new();
    public IReadOnlyCollection<SalesReturnLine> Lines => _lines.AsReadOnly();

    public decimal TotalReturnAmount => _lines.Sum(l => l.ReturnQuantity * l.UnitPrice);

    public void CalculateTotalAmount()
    {
        TotalAmount = _lines.Sum(l => l.ReturnedQuantity * l.OriginalItemCost);
    }

    private SalesReturn() { }

    public static SalesReturn Create(
        Guid companyId,
        Guid originalInvoiceId,
        Guid customerId,
        DateTime returnDate,
        string returnNumber,
        string returnReason)
    {
        return new SalesReturn
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            OriginalInvoiceId = originalInvoiceId,
            CustomerId = customerId,
            ReturnDate = returnDate,
            ReturnNumber = returnNumber,
            ReturnReason = returnReason,
            Status = SalesReturnStatus.Draft
        };
    }

    public void AddLine(Guid itemId, decimal returnQuantity, decimal unitPrice)
    {
        if (Status != SalesReturnStatus.Draft)
            throw new InvalidOperationException("Cannot modify approved Sales Return");

        _lines.Add(SalesReturnLine.Create(Id, itemId, returnQuantity, unitPrice));
    }

    public void Approve()
    {
        if (Status != SalesReturnStatus.Draft && Status != SalesReturnStatus.Pending)
            throw new InvalidOperationException($"Cannot approve Sales Return in status {Status}");

        if (!_lines.Any())
            throw new InvalidOperationException("Cannot approve Sales Return with no lines");

        Status = SalesReturnStatus.Approved;
    }
}

public class SalesReturnLine : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid SalesReturnId { get; private set; }
    public Guid SalesInvoiceLineId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal ReturnQuantity { get; private set; }
    public decimal ReturnedQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal OriginalItemCost { get; private set; }

    public void SetOriginalItemCost(decimal cost)
    {
        OriginalItemCost = cost;
    }

    private SalesReturnLine() { }

    internal static SalesReturnLine Create(Guid returnId, Guid itemId, decimal qty, decimal price)
        => new() { Id = Guid.NewGuid(), SalesReturnId = returnId, ItemId = itemId, ReturnQuantity = qty, ReturnedQuantity = qty, UnitPrice = price, OriginalItemCost = price };
}

public enum SalesReturnStatus 
{ 
    Draft = 0, 
    Pending = 1,
    Approved = 2, 
    Rejected = 3,
    Completed = 4,
    Cancelled = 5 
}
