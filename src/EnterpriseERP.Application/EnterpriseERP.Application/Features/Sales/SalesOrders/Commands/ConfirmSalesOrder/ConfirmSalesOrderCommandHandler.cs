using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Sales.SalesOrders.Commands.ConfirmSalesOrder;

public class ConfirmSalesOrderCommandHandler : IRequestHandler<ConfirmSalesOrderCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IInventoryReservationService _reservationService;
    private readonly ICurrentUserService _currentUserService;

    public ConfirmSalesOrderCommandHandler(
        IAppDbContext context,
        IInventoryReservationService reservationService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _reservationService = reservationService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ConfirmSalesOrderCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId ?? Guid.Empty;

        var salesOrder = await _context.SalesOrders
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == command.SalesOrderId 
                                   && so.CompanyId == command.CompanyId
                                   && so.TenantId == tenantId, cancellationToken);

        if (salesOrder == null)
            return Result.Failure(new Error("Sales.OrderNotFound", $"Sales Order {command.SalesOrderId} not found"));

        // 1. جلب بيانات الحد الائتماني للعميل
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == salesOrder.CustomerId 
                                   && c.CompanyId == command.CompanyId
                                   && c.TenantId == tenantId, cancellationToken);

        if (customer == null)
            return Result.Failure(new Error("Sales.CustomerNotFound", $"Customer {salesOrder.CustomerId} not found"));

        // 2. حساب الرصيد الحالي المستحق (Outstanding)
        var outstandingBalance = await _context.SalesInvoices
            .Where(i => i.CustomerId == salesOrder.CustomerId
                     && i.CompanyId == command.CompanyId
                     && i.TenantId == tenantId
                     && i.Status != InvoiceStatus.Paid
                     && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => i.RemainingAmount, cancellationToken);

        // 3. تأكيد الطلب
        try 
        {
            salesOrder.Confirm(customer.CreditLimit, outstandingBalance);
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Sales.ConfirmationError", ex.Message));
        }

        // 4. حجز المخزون لكل سطر
        foreach (var line in salesOrder.Lines)
        {
            var warehouseId = line.WarehouseId;
            if (!warehouseId.HasValue)
            {
                var defaultWarehouseResult = await GetDefaultWarehouseAsync(command.CompanyId, tenantId, cancellationToken);
                if (!defaultWarehouseResult.IsSuccess)
                    return Result.Failure(defaultWarehouseResult.Error);
                warehouseId = defaultWarehouseResult.Value;
            }

            // استخدام خدمة الحجز المركزية
            try 
            {
                var reservationId = await _reservationService.ReserveStockAsync(
                    line.ItemId,
                    warehouseId.Value,
                    line.OrderedQuantity,
                    salesOrder.Id,
                    "SalesOrder",
                    salesOrder.OrderNumber);

                line.SetReservedQuantity(line.OrderedQuantity);
                
                // إضافة سجل الحجز المحلي للـ Audit
                var reservation = ReservationEntry.Create(
                    command.CompanyId,
                    salesOrder.Id,
                    line.Id,
                    line.ItemId,
                    warehouseId.Value,
                    line.OrderedQuantity,
                    DateTime.UtcNow);
                
                reservation.TenantId = tenantId;
                _context.ReservationEntries.Add(reservation);
            }
            catch (Exception ex)
            {
                return Result.Failure(new Error("Inventory.ReservationError", $"Failed to reserve stock for item {line.ItemId}: {ex.Message}"));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<Guid>> GetDefaultWarehouseAsync(Guid companyId, Guid tenantId, CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.CompanyId == companyId && w.TenantId == tenantId && w.IsDefault, cancellationToken);
            
        if (warehouse == null)
            return Result.Failure<Guid>(new Error("Inventory.NoDefaultWarehouse", $"No default warehouse configured for company {companyId}"));
            
        return Result.Success(warehouse.Id);
    }
}
