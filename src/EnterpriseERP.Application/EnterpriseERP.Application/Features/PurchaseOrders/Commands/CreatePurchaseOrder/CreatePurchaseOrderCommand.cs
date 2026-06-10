using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommand : IRequest<Result<Guid>>
{
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public Guid SupplierId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<CreatePurchaseOrderLineRequest> Lines { get; set; } = new();
}

public class CreatePurchaseOrderLineRequest
{
    public Guid ItemId { get; set; }
    public Guid? ProjectTaskId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxPercent { get; set; }
    public string Notes { get; set; } = string.Empty;
}
