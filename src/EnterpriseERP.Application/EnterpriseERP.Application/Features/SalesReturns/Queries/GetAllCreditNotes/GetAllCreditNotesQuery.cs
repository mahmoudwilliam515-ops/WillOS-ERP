using EnterpriseERP.Application.Features.SalesReturns.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesReturns.Queries.GetAllCreditNotes;

public class GetAllCreditNotesQuery : IRequest<PagedResult<CreditNoteDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}
