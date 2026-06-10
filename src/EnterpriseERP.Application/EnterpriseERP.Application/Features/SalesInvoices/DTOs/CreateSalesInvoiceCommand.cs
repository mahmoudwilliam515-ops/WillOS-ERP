using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.DTOs;

public class SalesInvoiceLineRequest
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreateSalesInvoiceCommand : IRequest<SalesInvoiceDto>
{
    public DateTime InvoiceDate { get; set; }
    public Guid CustomerId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? DeliveryNoteId { get; set; }
    public decimal PaidAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<SalesInvoiceLineRequest> Lines { get; set; } = new();
}

