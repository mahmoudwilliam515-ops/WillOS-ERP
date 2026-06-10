namespace EnterpriseERP.Application.Features.Inventory.DTOs;

public class StockBalanceDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal CurrentBalance { get; set; }
}
