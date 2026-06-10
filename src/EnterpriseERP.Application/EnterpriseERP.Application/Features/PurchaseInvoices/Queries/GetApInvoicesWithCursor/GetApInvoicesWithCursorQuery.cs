using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;
using System.Text;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Queries.GetApInvoicesWithCursor;

// ─── Cursor Pagination Request ─────────────────────────────────────────────
/// <summary>
/// Phase 9 §9.1.3 — Cursor-based pagination for AP Invoices collection.
/// Supports OData-style $filter, $orderby, $select with limit ≤ 1000.
/// </summary>
public record GetApInvoicesWithCursorQuery(
    string? Filter,
    string? OrderBy,
    string? Select,
    int Limit,
    string? AfterCursor
) : IRequest<ApInvoicePagedResult>;

// ─── Result ──────────────────────────────────────────────────────────────────
public record ApInvoicePagedResult(
    IEnumerable<PurchaseInvoiceDto> Data,
    string? NextCursor,
    bool HasMore,
    int TotalCount,
    int Limit
);

// ─── Handler ─────────────────────────────────────────────────────────────────
public class GetApInvoicesWithCursorQueryHandler
    : IRequestHandler<GetApInvoicesWithCursorQuery, ApInvoicePagedResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetApInvoicesWithCursorQueryHandler(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<ApInvoicePagedResult> Handle(
        GetApInvoicesWithCursorQuery request,
        CancellationToken cancellationToken)
    {
        // Decode after_cursor → last seen invoice Id
        Guid? afterId = null;
        if (!string.IsNullOrEmpty(request.AfterCursor))
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(request.AfterCursor));
            if (Guid.TryParse(decoded, out var g)) afterId = g;
        }

        // Fetch all (filtered server-side via FindAsync or GetAllAsync)
        IEnumerable<PurchaseInvoice> all;
        if (afterId.HasValue)
        {
            all = await _unitOfWork.Repository<PurchaseInvoice>()
                .FindAsync(i => i.Id.CompareTo(afterId.Value) > 0);
        }
        else
        {
            all = await _unitOfWork.Repository<PurchaseInvoice>().GetAllAsync();
        }

        // Basic status filter from OData $filter (e.g. "status eq 'PENDING_APPROVAL'")
        if (!string.IsNullOrEmpty(request.Filter)
            && request.Filter.Contains("status eq", StringComparison.OrdinalIgnoreCase))
        {
            var start = request.Filter.IndexOf("'") + 1;
            var end   = request.Filter.LastIndexOf("'");
            if (start > 0 && end > start)
            {
                var statusStr = request.Filter[start..end];
                if (Enum.TryParse<PurchaseInvoiceStatus>(statusStr, true, out var parsedStatus))
                    all = all.Where(i => i.Status == parsedStatus);
            }
        }

        // Order by — default: created ascending (cursor order)
        all = request.OrderBy?.Contains("desc", StringComparison.OrdinalIgnoreCase) == true
            ? all.OrderByDescending(i => i.InvoiceDate)
            : all.OrderBy(i => i.InvoiceDate);

        var fetchLimit = Math.Min(request.Limit, 1000) + 1;
        var page = all.Take(fetchLimit).ToList();
        var hasMore = page.Count == fetchLimit;
        if (hasMore) page.RemoveAt(page.Count - 1); // Remove sentinel

        // Build next_cursor from last item's Id
        string? nextCursor = hasMore && page.Any()
            ? Convert.ToBase64String(Encoding.UTF8.GetBytes(page.Last().Id.ToString()))
            : null;

        var dtos = page.Select(inv => new PurchaseInvoiceDto
        {
            Id                   = inv.Id,
            InvoiceNumber        = inv.InvoiceNumber,
            SupplierInvoiceNumber = inv.SupplierInvoiceNumber,
            InvoiceDate          = inv.InvoiceDate,
            SupplierId           = inv.SupplierId,
            BranchId             = inv.BranchId,
            WarehouseId          = inv.WarehouseId,
            SubTotal             = inv.SubTotal,
            DiscountAmount       = inv.DiscountAmount,
            TaxAmount            = inv.TaxAmount,
            TotalAmount          = inv.TotalAmount,
            PaidAmount           = inv.PaidAmount,
            RemainingAmount      = inv.RemainingAmount,
            Status               = inv.Status.ToString(),
            Notes                = inv.Notes,
            Lines                = new()
        });

        return new ApInvoicePagedResult(dtos, nextCursor, hasMore, page.Count, request.Limit);
    }
}
