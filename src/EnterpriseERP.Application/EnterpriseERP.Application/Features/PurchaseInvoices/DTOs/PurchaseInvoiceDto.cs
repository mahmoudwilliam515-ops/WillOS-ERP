namespace EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;

public class PurchaseInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public Guid SupplierId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseInvoiceLineDto> Lines { get; set; } = new();
}

public class PurchaseInvoiceLineDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}
