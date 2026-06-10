namespace EnterpriseERP.Application.Features.SalesReturns.DTOs;

public class SalesReturnLineRequest
{
    public Guid SalesInvoiceLineId { get; set; }
    public Guid ItemId { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}
