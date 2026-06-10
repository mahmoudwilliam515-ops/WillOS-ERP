using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.SalesOrders.Commands;

// ─── Create Sales Order ───────────────────────────────────────────

public record CreateSalesOrderCommand : IRequest<Guid>
{
    public Guid CompanyId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime OrderDate { get; init; }
    public DateTime? RequestedDeliveryDate { get; init; }
    public string? CustomerReference { get; init; }
    public Guid? SalesPersonId { get; init; }
    public string? Notes { get; init; }
    public List<CreateSalesOrderLineDto> Lines { get; init; } = new();
}

public record CreateSalesOrderLineDto
{
    public Guid ItemId { get; init; }
    public decimal OrderedQuantity { get; init; }
    public decimal UnitPrice { get; init; }
    public Guid? WarehouseId { get; init; }
}

public class CreateSalesOrderCommandValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Sales Order must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId).NotEmpty();
            line.RuleFor(l => l.OrderedQuantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;

    public CreateSalesOrderCommandHandler(
        IAppDbContext context,
        IOrderNumberGenerator numberingService)
    {
        _context = context;
        _numberingService = numberingService;
    }

    public async Task<Guid> Handle(CreateSalesOrderCommand command, CancellationToken cancellationToken)
    {
        // التحقق من وجود العميل
        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == command.CustomerId, cancellationToken);

        if (!customerExists)
            throw new InvalidOperationException($"Customer {command.CustomerId} not found for company {command.CompanyId}");

        var orderNumber = await _numberingService.GenerateAsync("SO", command.CompanyId, Guid.Empty, cancellationToken);

        var salesOrder = SalesOrder.Create(
            command.CompanyId,
            command.CustomerId,
            command.OrderDate,
            orderNumber,
            command.RequestedDeliveryDate,
            command.CustomerReference,
            command.SalesPersonId,
            command.Notes);

        foreach (var line in command.Lines)
        {
            salesOrder.AddLine(line.ItemId, line.OrderedQuantity, line.UnitPrice, line.WarehouseId);
        }

        _context.SalesOrders.Add(salesOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return salesOrder.Id;
    }
}

// ─── Confirm Sales Order ──────────────────────────────────────────

public record ConfirmSalesOrderCommand : IRequest<Unit>
{
    public Guid SalesOrderId { get; init; }
    public Guid CompanyId { get; init; }
}

public class ConfirmSalesOrderCommandHandler : IRequestHandler<ConfirmSalesOrderCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IInventoryService _inventoryService;

    public ConfirmSalesOrderCommandHandler(
        IAppDbContext context,
        IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<Unit> Handle(ConfirmSalesOrderCommand command, CancellationToken cancellationToken)
    {
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == command.SalesOrderId && so.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Sales Order {command.SalesOrderId} not found");

        // جلب بيانات الحد الائتماني
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == salesOrder.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException($"Customer {salesOrder.CustomerId} not found");

        var outstandingBalance = await _context.SalesInvoices
            .Where(i => i.CustomerId == salesOrder.CustomerId
                     && i.CompanyId == command.CompanyId
                     && i.Status != InvoiceStatus.Paid
                     && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => i.RemainingAmount, cancellationToken);

        // Confirm — يطلق CreditLimitExceededException إذا تجاوز الحد
        salesOrder.Confirm(customer.CreditLimit, outstandingBalance);

        // حجز المخزون لكل سطر
        foreach (var line in salesOrder.Lines)
        {
            var warehouse = line.WarehouseId
                ?? await GetDefaultWarehouseAsync(cancellationToken);

            // فحص توفر المخزون
            var availableQty = await _inventoryService.GetAvailableQuantityAsync(
                line.ItemId, warehouse, command.CompanyId, cancellationToken);

            if (availableQty < line.OrderedQuantity)
                throw new InsufficientInventoryException(line.ItemId, line.OrderedQuantity, availableQty);

            var reservation = ReservationEntry.Create(
                command.CompanyId,
                salesOrder.Id,
                line.Id,
                line.ItemId,
                warehouse,
                line.OrderedQuantity,
                DateTime.UtcNow);

            _context.ReservationEntries.Add(reservation);
            line.SetReservedQuantity(line.OrderedQuantity);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task<Guid> GetDefaultWarehouseAsync(CancellationToken cancellationToken)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsMain && w.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("No main warehouse configured.");
        return warehouse.Id;
    }
}

// ─── Exceptions ───────────────────────────────────────────────────

public class InsufficientInventoryException : InvalidOperationException
{
    public InsufficientInventoryException(Guid itemId, decimal requested, decimal available)
        : base($"Insufficient inventory for item {itemId}. Requested: {requested}, Available: {available}") { }
}
