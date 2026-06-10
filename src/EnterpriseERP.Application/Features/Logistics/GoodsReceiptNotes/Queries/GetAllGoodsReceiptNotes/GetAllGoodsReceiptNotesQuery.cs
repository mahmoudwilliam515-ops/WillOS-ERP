using EnterpriseERP.Application.Features.Logistics.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Queries.GetAllGoodsReceiptNotes;

public class GetAllGoodsReceiptNotesQuery : IRequest<PagedResult<GoodsReceiptNoteDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}
