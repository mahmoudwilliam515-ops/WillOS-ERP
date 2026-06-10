using System;
using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.SharedKernel.Exceptions;
using EnterpriseERP.Domain.Common;
using EnterpriseERP.Domain.Exceptions;

namespace EnterpriseERP.Domain.Entities.Inventory;

/// <summary>
/// يُمثّل طبقة مخزون (Batch) لتقييم FIFO.
/// يتم إنشاء طبقة جديدة عند كل استلام بضاعة (GRN).
/// </summary>
public class InventoryFIFOLayer : AuditableEntity
{
    public Guid ItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid SourceDocumentId { get; private set; } // GRN Id
    
    public decimal OriginalQuantity { get; private set; }
    public decimal RemainingQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public DateTime ReceivedDate { get; private set; }

    // Navigation
    public Item Item { get; private set; } = null!;
    public Warehouse Warehouse { get; private set; } = null!;

    private InventoryFIFOLayer() { }

    public static InventoryFIFOLayer Create(
        Guid itemId,
        Guid warehouseId,
        Guid sourceDocumentId,
        decimal quantity,
        decimal unitCost,
        DateTime receivedDate,
        Guid tenantId)
    {
        if (quantity <= 0)
            throw new InventoryDomainException($"الكمية يجب أن تكون موجبة: {quantity}");

        return new InventoryFIFOLayer
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            WarehouseId = warehouseId,
            SourceDocumentId = sourceDocumentId,
            OriginalQuantity = quantity,
            RemainingQuantity = quantity,
            UnitCost = unitCost,
            ReceivedDate = receivedDate,
            TenantId = tenantId
        };
    }

    public void Consume(decimal quantity)
    {
        if (quantity <= 0)
            throw new InventoryDomainException($"كمية الاستهلاك يجب أن تكون موجبة: {quantity}");

        if (quantity > RemainingQuantity)
            throw new FIFOValuationException(
                $"الكمية المستهلكة ({quantity:N4}) تتجاوز المتبقي " +
                $"في الـ Layer ({RemainingQuantity:N4}).");

        RemainingQuantity -= quantity;
    }
}
