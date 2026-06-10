using EnterpriseERP.Application.Features.PurchaseReturns.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Queries.GetAllDebitNotes;

public class GetAllDebitNotesQuery : IRequest<PagedResult<DebitNoteDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}
