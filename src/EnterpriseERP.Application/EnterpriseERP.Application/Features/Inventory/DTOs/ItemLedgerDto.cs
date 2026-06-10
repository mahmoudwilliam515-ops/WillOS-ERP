namespace EnterpriseERP.Application.Features.Inventory.DTOs;

public class ItemLedgerDto
{
    public Guid TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal RunningBalance { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
