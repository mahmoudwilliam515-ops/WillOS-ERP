using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.SharedKernel.Exceptions;
using EnterpriseERP.Domain.Common;
using EnterpriseERP.Domain.Exceptions;

namespace EnterpriseERP.Domain.Entities.Inventory;

public class InventoryBalance : AuditableEntity
{
    public Guid ItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    
    public decimal AvailableQuantity { get; private set; }
    public decimal OnHandQuantity { get; private set; }
    
    public byte[] RowVersion { get; private set; } = null!;
    
    // Navigation
    public Item Item { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;

    private InventoryBalance() { }

    public static InventoryBalance Create(Guid itemId, Guid warehouseId, Guid tenantId)
    {
        return new InventoryBalance
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            WarehouseId = warehouseId,
            TenantId = tenantId,
            AvailableQuantity = 0,
            OnHandQuantity = 0
        };
    }

    public void Deduct(decimal quantity, Guid referenceId, DateTime referenceDate)
    {
        if (quantity <= 0)
            throw new InventoryDomainException($"الكمية يجب أن تكون موجبة: {quantity}");

        if (quantity > AvailableQuantity)
            throw new InsufficientInventoryException(
                $"الكمية المطلوبة ({quantity:N2}) تتجاوز المتاح ({AvailableQuantity:N2}) " +
                $"للمنتج {ItemId} في المستودع {WarehouseId}");

        AvailableQuantity -= quantity;
        OnHandQuantity -= quantity;
    }

    public void AddStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new InventoryDomainException($"الكمية يجب أن تكون موجبة: {quantity}");

        AvailableQuantity += quantity;
        OnHandQuantity += quantity;
    }
}
