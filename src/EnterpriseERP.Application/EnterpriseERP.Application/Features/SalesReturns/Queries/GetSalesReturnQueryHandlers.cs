using EnterpriseERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.SalesReturns.Queries;

// ── Sales Return — GetById ────────────────────────────────────────────────────

public class GetSalesReturnByIdQueryHandler
    : IRequestHandler<GetSalesReturnByIdQuery, SalesReturnDetailDto?>
{
    private readonly IAppDbContext _context;

    public GetSalesReturnByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReturnDetailDto?> Handle(
        GetSalesReturnByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.SalesReturns
            .Where(r => r.Id == request.Id && r.CompanyId == request.CompanyId)
            .Select(r => new SalesReturnDetailDto
            {
                Id = r.Id,
                CreditNoteNumber = r.ReturnNumber,
                CustomerId = r.CustomerId,
                CustomerName = "Customer " + r.CustomerId, // TODO: Fetch from customer service if needed
                ReturnDate = r.ReturnDate,
                Reason = r.ReturnReason,
                TotalAmount = r.TotalReturnAmount,
                Status = r.Status.ToString(),
                Lines = r.Lines.Select(l => new SalesReturnLineDto
                {
                    ItemId = l.ItemId,
                    ReturnedQuantity = l.ReturnQuantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.ReturnQuantity * l.UnitPrice
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

// ── Sales Return — GetList ─────────────────────────────────────────────────────

public class GetSalesReturnListQueryHandler
    : IRequestHandler<GetSalesReturnListQuery, IEnumerable<SalesReturnSummaryDto>>
{
    private readonly IAppDbContext _context;

    public GetSalesReturnListQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesReturnSummaryDto>> Handle(
        GetSalesReturnListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.SalesReturns
            .Where(r => r.CompanyId == request.CompanyId)
            .AsQueryable();

        if (request.CustomerId.HasValue)
            query = query.Where(r => r.CustomerId == request.CustomerId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(r => r.ReturnDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(r => r.ReturnDate <= request.ToDate.Value);

        return await query
            .OrderByDescending(r => r.ReturnDate)
            .Select(r => new SalesReturnSummaryDto
            {
                Id = r.Id,
                CreditNoteNumber = r.ReturnNumber,
                CustomerName = "Customer " + r.CustomerId,
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalReturnAmount
            })
            .ToListAsync(cancellationToken);
    }
}

// ── DTOs & Queries ─────────────────────────────────────────────────────────────

public record GetSalesReturnByIdQuery : IRequest<SalesReturnDetailDto?>
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
}

public record GetSalesReturnListQuery : IRequest<IEnumerable<SalesReturnSummaryDto>>
{
    public Guid CompanyId { get; init; }
    public Guid? CustomerId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public record SalesReturnDetailDto
{
    public Guid Id { get; init; }
    public string CreditNoteNumber { get; init; } = default!;
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = default!;
    public DateTime ReturnDate { get; init; }
    public string Reason { get; init; } = default!;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = default!;
    public List<SalesReturnLineDto> Lines { get; init; } = new();
}

public record SalesReturnLineDto
{
    public Guid ItemId { get; init; }
    public decimal ReturnedQuantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

public record SalesReturnSummaryDto
{
    public Guid Id { get; init; }
    public string CreditNoteNumber { get; init; } = default!;
    public string CustomerName { get; init; } = default!;
    public DateTime ReturnDate { get; init; }
    public decimal TotalAmount { get; init; }
}
