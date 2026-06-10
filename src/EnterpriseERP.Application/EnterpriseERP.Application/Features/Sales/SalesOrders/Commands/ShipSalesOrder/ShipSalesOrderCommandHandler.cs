using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Domain.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Sales.SalesOrders.Commands.ShipSalesOrder;

// ─── Command ──────────────────────────────────────────────────────────────────

public record ShipSalesOrderCommand : IRequest<Unit>
{
    public Guid SalesOrderId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid DeliveryNoteId { get; init; }
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public class ShipSalesOrderCommandHandler : IRequestHandler<ShipSalesOrderCommand, Unit>
{
    private readonly IAppDbContext _context;

    public ShipSalesOrderCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ShipSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var salesOrder = await _context.SalesOrders
            .Where(s => s.Id == request.SalesOrderId && s.CompanyId == request.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"SalesOrder {request.SalesOrderId} not found");

        // التحقق أن Delivery Note مكتملة
        var deliveryNote = await _context.DeliveryNotes
            .Where(d => d.Id == request.DeliveryNoteId && d.CompanyId == request.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"DeliveryNote {request.DeliveryNoteId} not found");

        if (deliveryNote.Status != DeliveryStatus.Completed)
            throw new InvalidOperationException(
                $"Cannot ship SalesOrder: DeliveryNote {request.DeliveryNoteId} status is {deliveryNote.Status}. Must be Completed.");

        salesOrder.Ship();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
