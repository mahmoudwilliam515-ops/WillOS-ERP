using EnterpriseERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Queries;

// ── Purchase Return — GetById ──────────────────────────────────────────────────

public class GetPurchaseReturnByIdQueryHandler
    : IRequestHandler<GetPurchaseReturnByIdQuery, PurchaseReturnDetailDto?>
{
    private readonly IAppDbContext _context;

    public GetPurchaseReturnByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PurchaseReturnDetailDto?> Handle(
        GetPurchaseReturnByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.PurchaseReturns
            .Where(r => r.Id == request.Id && r.CompanyId == request.CompanyId)
            .Select(r => new PurchaseReturnDetailDto
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                SupplierId = r.SupplierId,
                GRNId = r.GoodsReceiptNoteId,
                ReturnDate = r.ReturnDate,
                Reason = r.Reason,
                TotalAmount = r.TotalReturnValue,
                Status = r.Status.ToString(),
                DebitNoteId = null, // Not mapped yet
                Lines = r.Lines.Select(l => new PurchaseReturnLineDto
                {
                    ItemId = l.ItemId,
                    ReturnedQuantity = l.ReturnQuantity,
                    UnitCost = l.UnitCost,
                    LineTotal = l.ReturnQuantity * l.UnitCost
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

// ── Purchase Return — GetList ──────────────────────────────────────────────────

public class GetPurchaseReturnListQueryHandler
    : IRequestHandler<GetPurchaseReturnListQuery, IEnumerable<PurchaseReturnSummaryDto>>
{
    private readonly IAppDbContext _context;

    public GetPurchaseReturnListQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseReturnSummaryDto>> Handle(
        GetPurchaseReturnListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.PurchaseReturns
            .Where(r => r.CompanyId == request.CompanyId)
            .AsQueryable();

        if (request.SupplierId.HasValue)
            query = query.Where(r => r.SupplierId == request.SupplierId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(r => r.ReturnDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(r => r.ReturnDate <= request.ToDate.Value);

        return await query
            .OrderByDescending(r => r.ReturnDate)
            .Select(r => new PurchaseReturnSummaryDto
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                SupplierName = "Supplier " + r.SupplierId,
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalReturnValue,
                Status = r.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }
}

// ── DTOs & Queries ─────────────────────────────────────────────────────────────

public record GetPurchaseReturnByIdQuery : IRequest<PurchaseReturnDetailDto?>
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
}

public record GetPurchaseReturnListQuery : IRequest<IEnumerable<PurchaseReturnSummaryDto>>
{
    public Guid CompanyId { get; init; }
    public Guid? SupplierId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public record PurchaseReturnDetailDto
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = default!;
    public Guid SupplierId { get; init; }
    public Guid GRNId { get; init; }
    public DateTime ReturnDate { get; init; }
    public string Reason { get; init; } = default!;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = default!;
    public Guid? DebitNoteId { get; init; }
    public List<PurchaseReturnLineDto> Lines { get; init; } = new();
}

public record PurchaseReturnLineDto
{
    public Guid ItemId { get; init; }
    public decimal ReturnedQuantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal LineTotal { get; init; }
}

public record PurchaseReturnSummaryDto
{
    public Guid Id { get; init; }
    public string ReturnNumber { get; init; } = default!;
    public string SupplierName { get; init; } = default!;
    public DateTime ReturnDate { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = default!;
}
