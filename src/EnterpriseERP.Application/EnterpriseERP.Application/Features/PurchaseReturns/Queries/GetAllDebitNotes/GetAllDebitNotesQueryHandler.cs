using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.PurchaseReturns.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Queries.GetAllDebitNotes;

public class GetAllDebitNotesQueryHandler : IRequestHandler<GetAllDebitNotesQuery, PagedResult<DebitNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllDebitNotesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<DebitNoteDto>> Handle(GetAllDebitNotesQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchTerm?.Trim();
        var (items, totalCount) = await _unitOfWork.Repository<DebitNote>().GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            string.IsNullOrWhiteSpace(search)
                ? null
                : n => n.NoteNumber.Contains(search) || n.Reason.Contains(search));

        var list = items.ToList();
        var supplierIds = list.Select(n => n.SupplierId).Distinct().ToList();
        var suppliers = supplierIds.Count > 0
            ? (await _unitOfWork.Repository<Supplier>().FindAsync(s => supplierIds.Contains(s.Id)))
                .ToDictionary(s => s.Id, s => s.Name)
            : new Dictionary<Guid, string>();

        var returnIds = list.Where(n => n.PurchaseReturnId.HasValue).Select(n => n.PurchaseReturnId!.Value).Distinct().ToList();
        var returns = returnIds.Count > 0
            ? (await _unitOfWork.Repository<PurchaseReturn>().FindAsync(r => returnIds.Contains(r.Id)))
                .ToDictionary(r => r.Id, r => r.ReturnNumber)
            : new Dictionary<Guid, string>();

        var dtos = list.Select(n => new DebitNoteDto
        {
            Id = n.Id,
            NoteNumber = n.NoteNumber,
            NoteDate = n.NoteDate,
            SupplierId = n.SupplierId,
            SupplierName = suppliers.TryGetValue(n.SupplierId, out var sn) ? sn : string.Empty,
            PurchaseReturnId = n.PurchaseReturnId,
            ReturnNumber = n.PurchaseReturnId.HasValue && returns.TryGetValue(n.PurchaseReturnId.Value, out var rn) ? rn : null,
            Amount = n.Amount,
            Reason = n.Reason,
            Status = (int)n.Status,
            StatusName = n.Status == DebitNoteStatus.Approved ? "Approved" : "Draft"
        });

        return new PagedResult<DebitNoteDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
