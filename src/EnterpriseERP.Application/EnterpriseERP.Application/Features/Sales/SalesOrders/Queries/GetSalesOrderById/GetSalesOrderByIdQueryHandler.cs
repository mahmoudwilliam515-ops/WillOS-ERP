using EnterpriseERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Sales.SalesOrders.Queries.GetSalesOrderById;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetSalesOrderByIdQuery : IRequest<SalesOrderDto?>
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
}

// ─── DTO ──────────────────────────────────────────────────────────────────────

public class SalesOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? CustomerReference { get; set; }
    public string Status { get; set; } = default!;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public List<SalesOrderLineDto> Lines { get; set; } = new();
}

public class SalesOrderLineDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal ShippedQuantity { get; set; }
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public class GetSalesOrderByIdQueryHandler : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDto?>
{
    private readonly IAppDbContext _context;

    public GetSalesOrderByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SalesOrderDto?> Handle(GetSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var so = await _context.SalesOrders
            .Include(s => s.Lines)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.CompanyId == request.CompanyId, cancellationToken);

        if (so is null) return null;

        return new SalesOrderDto
        {
            Id = so.Id,
            OrderNumber = so.OrderNumber,
            CustomerId = so.CustomerId,
            OrderDate = so.OrderDate,
            RequestedDeliveryDate = so.RequestedDeliveryDate,
            CustomerReference = so.CustomerReference,
            Status = so.Status.ToString(),
            SubTotal = so.Lines.Sum(l => l.OrderedQuantity * l.UnitPrice),
            TaxAmount = 0,
            TotalAmount = so.Lines.Sum(l => l.OrderedQuantity * l.UnitPrice),
            Notes = so.Notes,
            Lines = so.Lines.Select(l => new SalesOrderLineDto
            {
                Id = l.Id,
                ItemId = l.ItemId,
                OrderedQuantity = l.OrderedQuantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.OrderedQuantity * l.UnitPrice,
                ReservedQuantity = l.ReservedQuantity,
                ShippedQuantity = l.ShippedQuantity
            }).ToList()
        };
    }
}
