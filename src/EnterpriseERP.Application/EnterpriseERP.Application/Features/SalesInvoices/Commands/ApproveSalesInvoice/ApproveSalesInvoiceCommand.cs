using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.Commands.ApproveSalesInvoice;

public record ApproveSalesInvoiceCommand(
    Guid InvoiceId,
    string ApprovedBy,
    Guid TenantId
) : IRequest<ApproveSalesInvoiceResult>;

public record ApproveSalesInvoiceResult(
    Guid InvoiceId,
    string InvoiceNumber,
    DateTime ApprovedAt
);
