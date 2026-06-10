using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.CreateGoodsReceiptNote;

public class GoodsReceiptLineRequest
{
    public Guid ItemId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
}

public class CreateGoodsReceiptNoteCommand : IRequest<Result<Guid>>
{
    public DateTime ReceiptDate { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<GoodsReceiptLineRequest> Lines { get; set; } = new();
}
