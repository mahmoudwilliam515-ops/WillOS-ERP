using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using EnterpriseERP.Domain.Entities.Manufacturing;

namespace EnterpriseERP.Application.Services;

public class InventoryPostingService : IInventoryPostingService
{
    public Task<IEnumerable<InventoryTransaction>> PostProductionCompletionAsync(ProductionOrder order, CancellationToken cancellationToken)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));

        var transactions = new List<InventoryTransaction>();

        // 1. Finished Product (In)
        transactions.Add(new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            ItemId = order.ProductId,
            WarehouseId = order.WarehouseId,
            ReferenceId = order.Id,
            ReferenceType = "ProductionOrder",
            ReferenceNumber = order.OrderNumber,
            Type = TransactionType.ManufacturingIn,
            Quantity = order.ProducedQuantity,
            UnitCost = order.ProducedQuantity > 0 ? order.TotalCost / order.ProducedQuantity : 0,
            TotalCost = order.TotalCost,
            TransactionDate = DateTime.UtcNow,
            Notes = $"Production completion {order.OrderNumber}"
        });

        // 2. Raw Materials (Out)
        foreach (var material in order.Materials)
        {
            transactions.Add(new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = material.RawMaterialId,
                WarehouseId = order.WarehouseId,
                ReferenceId = order.Id,
                ReferenceType = "ProductionOrder",
                ReferenceNumber = order.OrderNumber,
                Type = TransactionType.ManufacturingOut,
                Quantity = -material.ActualQuantity,
                UnitCost = material.UnitCost,
                TotalCost = material.ActualQuantity * material.UnitCost,
                TransactionDate = DateTime.UtcNow,
                Notes = $"Consumption for production {order.OrderNumber}"
            });
        }

        return Task.FromResult<IEnumerable<InventoryTransaction>>(transactions);
    }
    public Task<IEnumerable<InventoryTransaction>> PostSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        var transactions = new List<InventoryTransaction>();

        foreach (var line in invoice.Lines)
        {
            var inventoryTx = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = line.ItemId,
                WarehouseId = invoice.WarehouseId,
                ReferenceId = invoice.Id,
                ReferenceType = "SalesInvoice",
                ReferenceNumber = invoice.InvoiceNumber,
                Type = TransactionType.SalesOut,
                Quantity = -line.Quantity,   // Negative = stock reduction
                UnitCost = line.ItemCost,    // Actual computed cost of goods sold
                TotalCost = line.ExactTotalCost > 0 ? line.ExactTotalCost : line.ItemCost * line.Quantity,
                TransactionDate = invoice.InvoiceDate,
                Notes = $"Sales Invoice {invoice.InvoiceNumber}"
            };

            transactions.Add(inventoryTx);
        }

        return Task.FromResult<IEnumerable<InventoryTransaction>>(transactions);
    }

    public Task<IEnumerable<InventoryTransaction>> PostPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        // If invoice is linked to a GRN, stock was already increased at GRN stage.
        // In a true ERP system, PI only affects accounts payable, not stock if GRN exists.
        if (invoice.Lines.Any(l => l.GoodsReceiptLineId.HasValue))
        {
            return Task.FromResult<IEnumerable<InventoryTransaction>>(new List<InventoryTransaction>());
        }

        var transactions = new List<InventoryTransaction>();

        foreach (var line in invoice.Lines)
        {
            var inventoryTx = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = line.ItemId,
                WarehouseId = invoice.WarehouseId,
                ReferenceId = invoice.Id,
                ReferenceType = "PurchaseInvoice",
                ReferenceNumber = invoice.InvoiceNumber,
                Type = TransactionType.PurchaseIn,
                Quantity = line.Quantity,    // Positive = stock increase
                UnitCost = line.UnitCost,
                TotalCost = line.LineTotal,
                TransactionDate = invoice.InvoiceDate,
                Notes = $"Purchase Invoice {invoice.InvoiceNumber}"
            };

            transactions.Add(inventoryTx);
        }

        return Task.FromResult<IEnumerable<InventoryTransaction>>(transactions);
    }

    public Task<IEnumerable<InventoryTransaction>> PostGoodsReceiptNoteAsync(GoodsReceiptNote grn, CancellationToken cancellationToken)
    {
        if (grn == null) throw new ArgumentNullException(nameof(grn));

        var transactions = new List<InventoryTransaction>();

        foreach (var line in grn.Lines)
        {
            var inventoryTx = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = line.ItemId,
                WarehouseId = grn.WarehouseId,
                ReferenceId = grn.Id,
                ReferenceType = "GoodsReceiptNote",
                ReferenceNumber = grn.GRNNumber,
                Type = TransactionType.PurchaseIn,
                Quantity = line.ReceivedQuantity,   // Positive = stock increase
                UnitCost = line.UnitCost, // Capture unit cost from GRN line
                TotalCost = line.TotalCost,
                TransactionDate = grn.ReceiptDate,
                Notes = $"GRN {grn.GRNNumber}"
            };

            transactions.Add(inventoryTx);
        }

        return Task.FromResult<IEnumerable<InventoryTransaction>>(transactions);
    }

    public Task<IEnumerable<InventoryTransaction>> PostDeliveryNoteAsync(DeliveryNote dn, CancellationToken cancellationToken)
    {
        if (dn == null) throw new ArgumentNullException(nameof(dn));

        var transactions = new List<InventoryTransaction>();

        foreach (var line in dn.Lines)
        {
            var inventoryTx = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = line.ItemId,
                WarehouseId = dn.WarehouseId,
                ReferenceId = dn.Id,
                ReferenceType = "DeliveryNote",
                ReferenceNumber = dn.DeliveryNumber,
                Type = TransactionType.SalesOut,
                Quantity = -line.DeliveredQuantity, // Negative for stock out
                UnitCost = 0, 
                TotalCost = 0,
                TransactionDate = dn.DeliveryDate,
                Notes = $"Delivery Note {dn.DeliveryNumber}"
            };
            
            transactions.Add(inventoryTx);
        }

        return Task.FromResult<IEnumerable<InventoryTransaction>>(transactions);
    }

    public Task<IEnumerable<InventoryTransaction>> PostSalesReturnAsync(SalesReturn returnDoc, CancellationToken cancellationToken)
    {
        if (returnDoc == null) throw new ArgumentNullException(nameof(returnDoc));

        var transactions = new List<InventoryTransaction>();

        foreach (var line in returnDoc.Lines)
        {
            var inventoryTx = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = line.ItemId,
                WarehouseId = returnDoc.WarehouseId,
                ReferenceId = returnDoc.Id,
                ReferenceType = "SalesReturn",
                ReferenceNumber = returnDoc.ReturnNumber,
                Type = TransactionType.ReturnFromCustomer,
                Quantity = line.ReturnedQuantity,   // Positive = stock increase
                UnitCost = line.OriginalItemCost, // Use the actual COGS unit cost from the original invoice line
                TotalCost = line.ReturnedQuantity * line.OriginalItemCost, // Exactly reversed COGS
                TransactionDate = returnDoc.ReturnDate,
                Notes = $"Sales Return {returnDoc.ReturnNumber}"
            };

            transactions.Add(inventoryTx);
        }

        return Task.FromResult<IEnumerable<InventoryTransaction>>(transactions);
    }

    public Task<IEnumerable<InventoryTransaction>> PostPurchaseReturnAsync(PurchaseReturn returnDoc, CancellationToken cancellationToken)
    {
        if (returnDoc == null) throw new ArgumentNullException(nameof(returnDoc));

        var transactions = new List<InventoryTransaction>();

        foreach (var line in returnDoc.Lines)
        {
            transactions.Add(new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = line.ItemId,
                WarehouseId = returnDoc.WarehouseId,
                ReferenceId = returnDoc.Id,
                ReferenceType = "PurchaseReturn",
                ReferenceNumber = returnDoc.ReturnNumber,
                Type = TransactionType.ReturnToSupplier,
                Quantity = -line.ReturnedQuantity,
                UnitCost = line.UnitCost,
                TotalCost = line.ReturnedQuantity * line.UnitCost,
                TransactionDate = returnDoc.ReturnDate,
                Notes = $"Purchase Return {returnDoc.ReturnNumber}"
            });
        }

        return Task.FromResult<IEnumerable<InventoryTransaction>>(transactions);
    }

}
