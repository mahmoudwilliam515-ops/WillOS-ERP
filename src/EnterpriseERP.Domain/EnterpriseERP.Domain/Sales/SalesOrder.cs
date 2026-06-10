using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Sales.Events;
using EnterpriseERP.Domain.Entities.Sales;

namespace EnterpriseERP.Domain.Sales;

/// <summary>
/// أمر البيع — نقطة البداية في دورة O2C
/// Blueprint Section 1.2 — Order-to-Cash
/// State Machine: Draft → Confirmed → Shipped → Invoiced → Cancelled
/// </summary>
public class SalesOrder : AuditableEntity
{
    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public Guid CompanyId { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public DateTime? RequestedDeliveryDate { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public string? CustomerReference { get; private set; }
    public string? Notes { get; private set; }
    public Guid? SalesPersonId { get; private set; }

    // Credit Check Results — مُسجَّلة للمراجعة
    public bool CreditCheckPassed { get; private set; }
    public decimal CustomerCreditLimitAtConfirmation { get; private set; }
    public decimal CustomerOutstandingAtConfirmation { get; private set; }

    private readonly List<SalesOrderLine> _lines = new();
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();

    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    // Navigation
    public Customer Customer { get; private set; } = null!;

    private SalesOrder() { }

    public static SalesOrder Create(
        Guid companyId,
        Guid customerId,
        DateTime orderDate,
        string orderNumber,
        DateTime? requestedDeliveryDate = null,
        string? customerReference = null,
        Guid? salesPersonId = null,
        string? notes = null)
    {
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId is required");
        if (customerId == Guid.Empty) throw new ArgumentException("CustomerId is required");

        return new SalesOrder
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            CustomerId = customerId,
            OrderDate = orderDate,
            OrderNumber = orderNumber,
            RequestedDeliveryDate = requestedDeliveryDate,
            CustomerReference = customerReference,
            SalesPersonId = salesPersonId,
            Notes = notes,
            Status = SalesOrderStatus.Draft
        };
    }

    public void AddLine(
        Guid itemId,
        decimal orderedQuantity,
        decimal unitPrice,
        Guid? warehouseId = null,
        string? notes = null)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot add lines to Sales Order in status {Status}");

        if (orderedQuantity <= 0)
            throw new ArgumentException("Ordered quantity must be positive");

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative");

        _lines.Add(SalesOrderLine.Create(Id, itemId, orderedQuantity, unitPrice, warehouseId, notes));
    }

    /// <summary>
    /// تأكيد أمر البيع — يتطلب اجتياز فحص الحد الائتماني أولاً
    /// يُطلق SalesOrderConfirmedEvent لحجز المخزون
    /// </summary>
    public void Confirm(
        decimal customerCreditLimit,
        decimal customerOutstandingBalance)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot confirm Sales Order in status {Status}");

        if (!_lines.Any())
            throw new InvalidOperationException("Cannot confirm Sales Order with no lines");

        // Credit Limit Check — القاعدة الذهبية: لا تأكيد بدون فحص ائتماني
        var availableCredit = customerCreditLimit - customerOutstandingBalance;
        if (TotalAmount > availableCredit)
            throw new CreditLimitExceededException(
                CustomerId,
                TotalAmount,
                availableCredit,
                customerCreditLimit,
                customerOutstandingBalance);

        CreditCheckPassed = true;
        CustomerCreditLimitAtConfirmation = customerCreditLimit;
        CustomerOutstandingAtConfirmation = customerOutstandingBalance;
        Status = SalesOrderStatus.Confirmed;

        AddDomainEvent(new SalesOrderConfirmedEvent(Id, CompanyId, CustomerId, _lines.ToList()));
    }

    /// <summary>
    /// تأكيد الشحن — يُغيِّر الحالة بعد إنشاء Delivery Note مكتملة
    /// </summary>
    public void Ship()
    {
        if (Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot ship Sales Order in status {Status}. Must be Confirmed first.");

        Status = SalesOrderStatus.Shipped;
    }

    /// <summary>
    /// تأكيد الفوترة — بعد إصدار فاتورة المبيعات
    /// </summary>
    public void MarkAsInvoiced()
    {
        if (Status != SalesOrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot invoice Sales Order in status {Status}. Must be Shipped first.");

        Status = SalesOrderStatus.Invoiced;
    }

    public void Cancel(string reason)
    {
        if (Status == SalesOrderStatus.Invoiced)
            throw new InvalidOperationException("Cannot cancel an invoiced Sales Order. Create a Credit Note instead.");

        if (Status == SalesOrderStatus.Shipped)
            throw new InvalidOperationException("Cannot cancel a shipped Sales Order. Process a Sales Return first.");

        Status = SalesOrderStatus.Cancelled;
        AddDomainEvent(new SalesOrderCancelledEvent(Id, CompanyId, _lines.ToList()));
    }
}

public class SalesOrderLine : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal OrderedQuantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => OrderedQuantity * UnitPrice;
    public Guid? WarehouseId { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal ShippedQuantity { get; private set; }
    public string? Notes { get; private set; }

    public void UpdateShippedQuantity(decimal quantity)
    {
        ShippedQuantity += quantity;
    }

    private SalesOrderLine() { }

    internal static SalesOrderLine Create(
        Guid salesOrderId,
        Guid itemId,
        decimal orderedQuantity,
        decimal unitPrice,
        Guid? warehouseId,
        string? notes)
    {
        return new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            SalesOrderId = salesOrderId,
            ItemId = itemId,
            OrderedQuantity = orderedQuantity,
            UnitPrice = unitPrice,
            WarehouseId = warehouseId,
            Notes = notes
        };
    }

    public void SetReservedQuantity(decimal qty) => ReservedQuantity = qty;
}

public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Shipped = 2,
    Invoiced = 3,
    Cancelled = 4
}

public enum DeliveryNoteStatus
{
    Draft = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

/// <summary>
/// استثناء تجاوز الحد الائتماني — استثناء صريح وفق القاعدة الذهبية
/// </summary>
public class CreditLimitExceededException : InvalidOperationException
{
    public Guid CustomerId { get; }
    public decimal OrderAmount { get; }
    public decimal AvailableCredit { get; }

    public CreditLimitExceededException(
        Guid customerId,
        decimal orderAmount,
        decimal availableCredit,
        decimal creditLimit,
        decimal outstandingBalance)
        : base($"Customer {customerId} credit limit exceeded. " +
               $"Order amount: {orderAmount:N2}, Available credit: {availableCredit:N2} " +
               $"(Limit: {creditLimit:N2}, Outstanding: {outstandingBalance:N2})")
    {
        CustomerId = customerId;
        OrderAmount = orderAmount;
        AvailableCredit = availableCredit;
    }
}
