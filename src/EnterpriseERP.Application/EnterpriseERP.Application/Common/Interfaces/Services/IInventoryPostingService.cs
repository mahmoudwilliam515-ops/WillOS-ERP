using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Sales;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IInventoryPostingService
{
    Task<IEnumerable<InventoryTransaction>> PostProductionCompletionAsync(ProductionOrder order, CancellationToken cancellationToken);

    /// <summary>
    /// Generates inventory transactions for a Sales Invoice (Stock Out).
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> PostSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken);

    /// <summary>
    /// Generates inventory transactions for a Purchase Invoice (Stock In).
    /// </summary>
    Task<IEnumerable<InventoryTransaction>> PostPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken);
    
    Task<IEnumerable<InventoryTransaction>> PostSalesReturnAsync(SalesReturn returnDoc, CancellationToken cancellationToken);

    Task<IEnumerable<InventoryTransaction>> PostPurchaseReturnAsync(PurchaseReturn returnDoc, CancellationToken cancellationToken);

    Task<IEnumerable<InventoryTransaction>> PostGoodsReceiptNoteAsync(GoodsReceiptNote grn, CancellationToken cancellationToken);
    
    Task<IEnumerable<InventoryTransaction>> PostDeliveryNoteAsync(DeliveryNote dn, CancellationToken cancellationToken);
}
