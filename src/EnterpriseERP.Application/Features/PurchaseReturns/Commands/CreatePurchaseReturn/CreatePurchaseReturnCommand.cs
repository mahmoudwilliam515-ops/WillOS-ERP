using EnterpriseERP.Application.Features.PurchaseReturns.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Commands.CreatePurchaseReturn;

public class CreatePurchaseReturnCommand : IRequest<Guid>
{
    public DateTime ReturnDate { get; set; }
    public Guid PurchaseInvoiceId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal TaxPercent { get; set; }
    public List<PurchaseReturnLineRequest> Lines { get; set; } = new();
}
