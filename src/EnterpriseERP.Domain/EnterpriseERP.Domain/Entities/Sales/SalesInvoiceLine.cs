using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Inventory;

namespace EnterpriseERP.Domain.Entities.Sales;

public class SalesInvoiceLine : AuditableEntity
{
    public Guid SalesInvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public string ItemName { get; set; } = string.Empty;
    public Guid? WarehouseId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }   // (Qty * UnitPrice) - Discount + Tax
    public decimal ItemCost { get; set; }    // Track the actual cost of the item at the time of sale
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal ExactTotalCost { get; set; } // Tracks the exact unrounded total cost for the line

    public string Notes { get; set; } = string.Empty;

    // Navigation
    public SalesInvoice SalesInvoice { get; set; } = null!;
    
    public static SalesInvoiceLine Create(
        Guid salesInvoiceId, Guid itemId, string itemName,
        decimal quantity, decimal unitPrice,
        decimal discountPercent = 0, decimal taxPercent = 0,
        Guid? warehouseId = null, Guid? tenantId = null)
    {
        return new SalesInvoiceLine
        {
            SalesInvoiceId = salesInvoiceId,
            ItemId = itemId,
            ItemName = itemName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent,
            TaxPercent = taxPercent,
            WarehouseId = warehouseId,
            TenantId = tenantId ?? Guid.Empty
        };
    }
}
