using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class SalesFulfillmentService : ISalesFulfillmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryPostingService _inventoryPostingService;
    private readonly IInventoryReservationService _reservationService;
    private readonly IAccountingPostingService _accountingPostingService;

    public SalesFulfillmentService(
        IUnitOfWork unitOfWork,
        IInventoryPostingService inventoryPostingService,
        IInventoryReservationService reservationService,
        IAccountingPostingService accountingPostingService)
    {
        _unitOfWork = unitOfWork;
        _inventoryPostingService = inventoryPostingService;
        _reservationService = reservationService;
        _accountingPostingService = accountingPostingService;
    }

    public async Task<Guid> CreateDeliveryFromOrderAsync(Guid salesOrderId, DateTime deliveryDate)
    {
        var order = await _unitOfWork.Repository<EnterpriseERP.Domain.Sales.SalesOrder>().Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == salesOrderId);

        if (order == null)
            throw new SalesDomainException("Sales Order not found.");

        if (order.Status != EnterpriseERP.Domain.Sales.SalesOrderStatus.Confirmed
         && order.Status != EnterpriseERP.Domain.Sales.SalesOrderStatus.Shipped)
            throw new SalesDomainException("Order must be confirmed before delivery.");

        // 1. Create Delivery Note using factory method (private setters)
        var warehouseId = order.Lines.FirstOrDefault()?.WarehouseId ?? Guid.Empty;
        var deliveryNumber = $"DN-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var delivery = EnterpriseERP.Domain.Sales.DeliveryNote.Create(
            companyId: order.CompanyId,
            salesOrderId: order.Id,
            customerId: order.CustomerId,
            warehouseId: warehouseId,
            deliveryDate: deliveryDate,
            deliveryNumber: deliveryNumber);

        foreach (var line in order.Lines)
        {
            delivery.AddLine(line.Id, line.ItemId, line.OrderedQuantity);
        }

        delivery.Complete();

        // 2. Post Inventory Transactions (Deduct Stock)
        foreach (var line in delivery.Lines)
        {
            var tx = new InventoryTransaction
            {
                Id = Guid.NewGuid(),
                ItemId = line.ItemId,
                WarehouseId = delivery.WarehouseId,
                ReferenceId = delivery.Id,
                ReferenceType = "DeliveryNote",
                ReferenceNumber = deliveryNumber,
                Type = TransactionType.SalesOut,
                Quantity = -line.DeliveredQuantity,
                TransactionDate = deliveryDate,
                Notes = $"Delivery for Order {order.OrderNumber}"
            };
            await _unitOfWork.Repository<InventoryTransaction>().AddAsync(tx);
        }

        // 3. Fulfill Reservations
        await _reservationService.FulfillReservationAsync(order.Id, "SalesOrder");

        await _unitOfWork.Repository<EnterpriseERP.Domain.Sales.DeliveryNote>().AddAsync(delivery);
        await _unitOfWork.SaveChangesAsync(default);

        return delivery.Id;
    }

    public async Task<Guid> CreateInvoiceFromDeliveryAsync(Guid deliveryNoteId, DateTime invoiceDate)
    {
        var delivery = await _unitOfWork.Repository<EnterpriseERP.Domain.Sales.DeliveryNote>().Query()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == deliveryNoteId);

        if (delivery == null)
            throw new SalesDomainException("Delivery Note not found.");

        if (delivery.SalesInvoiceId.HasValue)
            throw new SalesDomainException("Delivery Note is already invoiced.");

        // 1. Create Sales Invoice
        var invoice = new SalesInvoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            InvoiceDate = invoiceDate,
            CustomerId = delivery.CustomerId,
            WarehouseId = delivery.WarehouseId,
            DeliveryNoteId = delivery.Id,
            Status = InvoiceStatus.Approved,
            Lines = delivery.Lines.Select(l => new SalesInvoiceLine
            {
                Id = Guid.NewGuid(),
                ItemId = l.ItemId,
                Quantity = l.DeliveredQuantity,
                UnitPrice = 0,
                ItemCost = 0
            }).ToList()
        };

        // 2. Post Accounting Entry
        await _accountingPostingService.PostSalesInvoiceAsync(invoice, default);

        await _unitOfWork.Repository<SalesInvoice>().AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync(default);

        return invoice.Id;
    }
}

