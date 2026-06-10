using EnterpriseERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Sales.SalesOrders.Queries.GetSalesOrderList;

// ─── Query ────────────────────────────────────────────────────────────────────

public record GetSalesOrderListQuery : IRequest<List<SalesOrderListItemDto>>
{
    public Guid CompanyId { get; init; }
    public Guid? CustomerId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// ─── DTO ──────────────────────────────────────────────────────────────────────

public class SalesOrderListItemDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public int LineCount { get; set; }
}

// ─── Handler ──────────────────────────────────────────────────────────────────

public class GetSalesOrderListQueryHandler : IRequestHandler<GetSalesOrderListQuery, List<SalesOrderListItemDto>>
{
    private readonly IAppDbContext _context;

    public GetSalesOrderListQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalesOrderListItemDto>> Handle(GetSalesOrderListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SalesOrders
            .Include(s => s.Lines)
            .AsNoTracking()
            .Where(s => s.CompanyId == request.CompanyId);

        if (request.CustomerId.HasValue)
            query = query.Where(s => s.CustomerId == request.CustomerId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(s => s.OrderDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(s => s.OrderDate <= request.ToDate.Value);

        var orders = await query
            .OrderByDescending(s => s.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return orders.Select(s => new SalesOrderListItemDto
        {
            Id = s.Id,
            OrderNumber = s.OrderNumber,
            CustomerId = s.CustomerId,
            OrderDate = s.OrderDate,
            Status = s.Status.ToString(),
            TotalAmount = s.Lines.Sum(l => l.OrderedQuantity * l.UnitPrice),
            LineCount = s.Lines.Count
        }).ToList();
    }
}
