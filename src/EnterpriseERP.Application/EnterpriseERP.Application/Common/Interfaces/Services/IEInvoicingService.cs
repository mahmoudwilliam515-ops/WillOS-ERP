using EnterpriseERP.Application.Features.SalesInvoices.DTOs;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IEInvoicingService
{
    Task<EInvoicePreviewDto> GetPreviewAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Generates ZATCA/ETA compliant XML for the invoice.</summary>
    Task<string> GenerateInvoiceXmlAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Submits the invoice to the tax authority gateway (or sandbox).</summary>
    Task<EInvoiceSubmissionResultDto> SubmitInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
