using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using MediatR;
using EnterpriseERP.SharedKernel.Results;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;

public class PurchaseInvoiceLineRequest
{
    public Guid ItemId { get; set; }
    public Guid? PurchaseOrderLineId { get; set; }
    public Guid? GoodsReceiptLineId { get; set; }
    public Guid? ProjectTaskId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreatePurchaseInvoiceCommand : IRequest<Result<PurchaseInvoiceDto>>
{
    public DateTime InvoiceDate { get; set; }
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal PaidAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseInvoiceLineRequest> Lines { get; set; } = new();
}
