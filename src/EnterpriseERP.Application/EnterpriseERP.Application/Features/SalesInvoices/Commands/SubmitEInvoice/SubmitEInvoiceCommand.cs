using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.Commands.SubmitEInvoice;

public record SubmitEInvoiceCommand(Guid InvoiceId) : IRequest<EInvoiceSubmissionResultDto>;

public class SubmitEInvoiceCommandHandler : IRequestHandler<SubmitEInvoiceCommand, EInvoiceSubmissionResultDto>
{
    private readonly IEInvoicingService _eInvoicingService;

    public SubmitEInvoiceCommandHandler(IEInvoicingService eInvoicingService)
    {
        _eInvoicingService = eInvoicingService;
    }

    public Task<EInvoiceSubmissionResultDto> Handle(SubmitEInvoiceCommand request, CancellationToken cancellationToken) =>
        _eInvoicingService.SubmitInvoiceAsync(request.InvoiceId, cancellationToken);
}
