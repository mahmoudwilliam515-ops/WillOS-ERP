using MediatR;
using EnterpriseERP.Domain.Enums;

namespace EnterpriseERP.Application.Features.Treasury.Commands.SettleInvoicePayment;

public record SettleInvoicePaymentCommand(
    Guid ReceiptVoucherId,
    Guid SalesInvoiceId,
    decimal AllocatedAmount,
    DateTime SettlementDate,
    Guid TenantId,
    string? Notes = null
) : IRequest<SettleInvoicePaymentResult>;

public record SettleInvoicePaymentResult(
    Guid InvoicePaymentId,
    decimal RemainingAmountAfter,
    InvoiceSettlementStatus NewStatus
);
