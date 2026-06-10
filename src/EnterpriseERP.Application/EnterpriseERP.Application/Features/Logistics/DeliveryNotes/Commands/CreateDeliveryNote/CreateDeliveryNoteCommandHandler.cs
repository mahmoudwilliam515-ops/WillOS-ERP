using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Entities.Sales;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.DeliveryNotes.Commands.CreateDeliveryNote;

// ═══════════════════════════════════════════════════════════
// Command
// ═══════════════════════════════════════════════════════════

public record CreateDeliveryNoteCommand : IRequest<Guid>
{
    public Guid CompanyId { get; init; }
    public Guid SalesOrderId { get; init; }
    public Guid WarehouseId { get; init; }
    public DateTime DeliveryDate { get; init; }
    public string? ShipmentReference { get; init; }
    public string? Notes { get; init; }
    public List<DeliveryNoteLineDto> Lines { get; init; } = new();
}

public record DeliveryNoteLineDto
{
    public Guid SalesOrderLineId { get; init; }
    public Guid ItemId { get; init; }
    public decimal ShippedQuantity { get; init; }
    public string? LotNumber { get; init; }
}

// ═══════════════════════════════════════════════════════════
// Validator
// ═══════════════════════════════════════════════════════════

public class CreateDeliveryNoteCommandValidator : AbstractValidator<CreateDeliveryNoteCommand>
{
    public CreateDeliveryNoteCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.DeliveryDate).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Delivery note must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.SalesOrderLineId).NotEmpty();
            line.RuleFor(l => l.ItemId).NotEmpty();
            line.RuleFor(l => l.ShippedQuantity).GreaterThan(0);
        });
    }
}

// ═══════════════════════════════════════════════════════════
// Handler
// ═══════════════════════════════════════════════════════════

public class CreateDeliveryNoteCommandHandler : IRequestHandler<CreateDeliveryNoteCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;

    public CreateDeliveryNoteCommandHandler(
        IAppDbContext context,
        IOrderNumberGenerator numberingService)
    {
        _context = context;
        _numberingService = numberingService;
    }

    public async Task<Guid> Handle(CreateDeliveryNoteCommand command, CancellationToken cancellationToken)
    {
        // 1. التحقق من Sales Order
        var salesOrder = await _context.SalesOrders
            .Include(so => so.Lines)
            .FirstOrDefaultAsync(so => so.Id == command.SalesOrderId && so.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"SalesOrder {command.SalesOrderId} not found.");

        if (salesOrder.Status != EnterpriseERP.Domain.Sales.SalesOrderStatus.Confirmed && salesOrder.Status != EnterpriseERP.Domain.Sales.SalesOrderStatus.Shipped)
            throw new InvalidOperationException(
                $"Cannot create delivery note for SalesOrder in status {salesOrder.Status}. Must be Confirmed.");

        // 2. التحقق من الكميات المتاحة (مقارنة بالمحجوز)
        foreach (var line in command.Lines)
        {
            var soLine = salesOrder.Lines.FirstOrDefault(l => l.Id == line.SalesOrderLineId)
                ?? throw new InvalidOperationException($"SalesOrderLine {line.SalesOrderLineId} not found.");

            // التحقق أن الكمية المشحونة لا تتجاوز الكمية المطلوبة
            if (line.ShippedQuantity > soLine.OrderedQuantity)
                throw new InvalidOperationException(
                    $"Shipped quantity {line.ShippedQuantity} exceeds ordered quantity for item {line.ItemId}.");
        }

        // 3. توليد رقم تسلسلي
        var deliveryNoteNumber = await _numberingService.GenerateAsync("DN", command.CompanyId, Guid.Empty, cancellationToken);

        // 4. إنشاء Delivery Note
        var deliveryNote = DeliveryNote.Create(
            command.CompanyId,
            command.SalesOrderId,
            salesOrder.CustomerId,
            command.WarehouseId,
            command.DeliveryDate,
            deliveryNoteNumber,
            command.ShipmentReference,
            command.Notes);

        foreach (var line in command.Lines)
        {
            deliveryNote.AddLine(line.SalesOrderLineId, line.ItemId, line.ShippedQuantity);
        }

        // 5. اكتمال Delivery Note
        deliveryNote.Complete();

        // 6. تحرير حجوزات المخزون (ReservationEntries)
        var reservations = await _context.ReservationEntries
            .Where(r => r.SalesOrderId == command.SalesOrderId
                     && r.CompanyId == command.CompanyId
                     && r.Status == ReservationStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var line in command.Lines)
        {
            var reservation = reservations.FirstOrDefault(r => r.SalesOrderLineId == line.SalesOrderLineId);
            reservation?.Fulfill();
        }

        _context.DeliveryNotes.Add(deliveryNote);
        await _context.SaveChangesAsync(cancellationToken);

        return deliveryNote.Id;
    }
}
