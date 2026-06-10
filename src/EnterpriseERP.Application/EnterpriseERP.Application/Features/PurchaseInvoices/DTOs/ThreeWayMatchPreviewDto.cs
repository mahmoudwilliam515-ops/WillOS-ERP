namespace EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;

public class ThreeWayMatchPreviewDto
{
    public Guid PurchaseInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public List<string> GoodsReceiptNumbers { get; set; } = new();
    public bool CanMatch { get; set; }
    public string SummaryMessage { get; set; } = string.Empty;
    public List<ThreeWayMatchLineDto> Lines { get; set; } = new();
}

public class ThreeWayMatchLineDto
{
    public Guid InvoiceLineId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;

    public decimal InvoiceQuantity { get; set; }
    public decimal InvoiceUnitCost { get; set; }
    public decimal InvoiceLineTotal { get; set; }

    public decimal? PoQuantity { get; set; }
    public decimal? PoUnitCost { get; set; }
    public string? PoLineReference { get; set; }

    public decimal? GrnReceivedQuantity { get; set; }
    public string? GrnNumber { get; set; }

    public bool HasQuantityVariance { get; set; }
    public bool HasPriceVariance { get; set; }
    public bool MissingPoLink { get; set; }
    public bool MissingGrnLink { get; set; }
    public List<string> Issues { get; set; } = new();
}
