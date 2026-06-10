namespace EnterpriseERP.Application.Features.Logistics.DTOs;

public class GoodsReceiptNoteDto
{
    public Guid Id { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int LineCount { get; set; }
}
