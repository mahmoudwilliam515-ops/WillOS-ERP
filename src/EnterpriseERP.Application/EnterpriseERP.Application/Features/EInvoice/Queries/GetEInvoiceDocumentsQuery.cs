using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.EInvoice;
using EnterpriseERP.SharedKernel.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.EInvoice.Queries;

public record EInvoiceDocumentDto(Guid Id, string Provider, string Status, string Uuid, string AuthorityResponseCode, string AuthorityResponseMessage, DateTime? SubmittedAt);

public class GetEInvoiceDocumentsQuery : IRequest<List<EInvoiceDocumentDto>>
{
}

public class GetEInvoiceDocumentsQueryHandler : IRequestHandler<GetEInvoiceDocumentsQuery, List<EInvoiceDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEInvoiceDocumentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<EInvoiceDocumentDto>> Handle(GetEInvoiceDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await _unitOfWork.Repository<EInvoiceDocument>().GetAllAsync();
        
        return docs.Select(d => new EInvoiceDocumentDto(
            d.Id,
            d.Provider.ToString(),
            d.Status.ToString(),
            d.Uuid,
            d.AuthorityResponseCode,
            d.AuthorityResponseMessage,
            d.SubmittedAt
        )).OrderByDescending(x => x.SubmittedAt ?? DateTime.MinValue).ToList();
    }
}
