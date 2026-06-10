using EnterpriseERP.Domain.Entities.Inventory;

namespace EnterpriseERP.Domain.Services;

public interface IFIFOValuationService
{
    IReadOnlyList<FIFOConsumption> CalculateFIFOCost(
        IEnumerable<InventoryFIFOLayer> fifoLayers,
        decimal quantityToDeduct,
        Guid itemId);

    decimal CalculateAverageUnitCost(
        IEnumerable<InventoryFIFOLayer> fifoLayers,
        Guid itemId);
}
