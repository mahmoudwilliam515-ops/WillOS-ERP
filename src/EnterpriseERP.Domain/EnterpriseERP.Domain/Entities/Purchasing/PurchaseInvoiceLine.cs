using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Inventory;

namespace EnterpriseERP.Domain.Entities.Purchasing;

public class PurchaseInvoiceLine : BaseEntity
{
    public Guid PurchaseInvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public Guid? PurchaseOrderLineId { get; set; }
    public Guid? GoodsReceiptLineId { get; set; }
    public Guid? ProjectTaskId { get; set; } // Link to Project Task for actual cost

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice => UnitCost;   // alias
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public string Notes { get; set; } = string.Empty;

    // Navigation
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
