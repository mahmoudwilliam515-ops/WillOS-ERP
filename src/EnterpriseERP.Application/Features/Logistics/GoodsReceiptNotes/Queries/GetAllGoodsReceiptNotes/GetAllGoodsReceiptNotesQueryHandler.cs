using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Logistics.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Queries.GetAllGoodsReceiptNotes;

public class GetAllGoodsReceiptNotesQueryHandler : IRequestHandler<GetAllGoodsReceiptNotesQuery, PagedResult<GoodsReceiptNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllGoodsReceiptNotesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<GoodsReceiptNoteDto>> Handle(GetAllGoodsReceiptNotesQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchTerm?.Trim();
        var (items, totalCount) = await _unitOfWork.Repository<GoodsReceiptNote>().GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            string.IsNullOrWhiteSpace(search)
                ? null
                : g => g.GRNNumber.Contains(search) || g.ReferenceNumber.Contains(search));

        var list = items.ToList();
        var supplierIds = list.Select(g => g.SupplierId).Distinct().ToList();
        var suppliers = supplierIds.Count > 0
            ? (await _unitOfWork.Repository<Supplier>().FindAsync(s => supplierIds.Contains(s.Id)))
                .ToDictionary(s => s.Id, s => s.Name)
            : new Dictionary<Guid, string>();

        var grnIds = list.Select(g => g.Id).ToList();
        var lines = grnIds.Count > 0
            ? await _unitOfWork.Repository<GoodsReceiptLine>().FindAsync(l => grnIds.Contains(l.GoodsReceiptNoteId))
            : Enumerable.Empty<GoodsReceiptLine>();
        var lineCounts = lines.GroupBy(l => l.GoodsReceiptNoteId).ToDictionary(g => g.Key, g => g.Count());

        var dtos = list.Select(g => new GoodsReceiptNoteDto
        {
            Id = g.Id,
            GRNNumber = g.GRNNumber,
            ReceiptDate = g.ReceiptDate,
            PurchaseOrderId = g.PurchaseOrderId,
            SupplierId = g.SupplierId,
            SupplierName = suppliers.TryGetValue(g.SupplierId, out var name) ? name : string.Empty,
            WarehouseId = g.WarehouseId,
            ReferenceNumber = g.ReferenceNumber,
            Status = (int)g.Status,
            StatusName = g.Status.ToString(),
            LineCount = lineCounts.TryGetValue(g.Id, out var count) ? count : 0
        });

        return new PagedResult<GoodsReceiptNoteDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
