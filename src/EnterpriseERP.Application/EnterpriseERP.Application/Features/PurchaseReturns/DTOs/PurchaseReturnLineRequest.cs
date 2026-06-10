namespace EnterpriseERP.Application.Features.PurchaseReturns.DTOs;

public class PurchaseReturnLineRequest
{
    public Guid PurchaseInvoiceLineId { get; set; }
    public Guid ItemId { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public decimal UnitCost { get; set; }
}
