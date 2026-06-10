using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.Queries.GetEInvoicePreview;

public record GetEInvoicePreviewQuery(Guid InvoiceId) : IRequest<EInvoicePreviewDto>;

public class GetEInvoicePreviewQueryHandler : IRequestHandler<GetEInvoicePreviewQuery, EInvoicePreviewDto>
{
    private readonly IEInvoicingService _eInvoicingService;

    public GetEInvoicePreviewQueryHandler(IEInvoicingService eInvoicingService)
    {
        _eInvoicingService = eInvoicingService;
    }

    public Task<EInvoicePreviewDto> Handle(GetEInvoicePreviewQuery request, CancellationToken cancellationToken) =>
        _eInvoicingService.GetPreviewAsync(request.InvoiceId, cancellationToken);
}
