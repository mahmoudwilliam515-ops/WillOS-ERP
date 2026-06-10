using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.SalesReturns.DTOs;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesReturns.Queries.GetAllCreditNotes;

public class GetAllCreditNotesQueryHandler : IRequestHandler<GetAllCreditNotesQuery, PagedResult<CreditNoteDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCreditNotesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CreditNoteDto>> Handle(GetAllCreditNotesQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchTerm?.Trim();
        var (items, totalCount) = await _unitOfWork.Repository<CreditNote>().GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            string.IsNullOrWhiteSpace(search)
                ? null
                : n => n.NoteNumber.Contains(search) || n.Reason.Contains(search));

        var list = items.ToList();
        var customerIds = list.Select(n => n.CustomerId).Distinct().ToList();
        var customers = customerIds.Count > 0
            ? (await _unitOfWork.Repository<Customer>().FindAsync(c => customerIds.Contains(c.Id)))
                .ToDictionary(c => c.Id, c => c.Name)
            : new Dictionary<Guid, string>();

        var returnIds = list.Where(n => n.SalesReturnId.HasValue).Select(n => n.SalesReturnId!.Value).Distinct().ToList();
        var returns = returnIds.Count > 0
            ? (await _unitOfWork.Repository<SalesReturn>().FindAsync(r => returnIds.Contains(r.Id)))
                .ToDictionary(r => r.Id, r => r.ReturnNumber)
            : new Dictionary<Guid, string>();

        var dtos = list.Select(n => new CreditNoteDto
        {
            Id = n.Id,
            NoteNumber = n.NoteNumber,
            NoteDate = n.NoteDate,
            CustomerId = n.CustomerId,
            CustomerName = customers.TryGetValue(n.CustomerId, out var cn) ? cn : string.Empty,
            SalesReturnId = n.SalesReturnId,
            ReturnNumber = n.SalesReturnId.HasValue && returns.TryGetValue(n.SalesReturnId.Value, out var rn) ? rn : null,
            Amount = n.Amount,
            Reason = n.Reason,
            Status = n.Status,
            StatusName = n.Status == 1 ? "Approved" : "Draft"
        });

        return new PagedResult<CreditNoteDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
