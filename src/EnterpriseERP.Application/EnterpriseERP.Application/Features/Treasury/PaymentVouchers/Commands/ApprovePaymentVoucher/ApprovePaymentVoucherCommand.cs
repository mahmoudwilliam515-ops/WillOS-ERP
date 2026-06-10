using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Commands.ApprovePaymentVoucher;

public record ApprovePaymentVoucherCommand(Guid VoucherId) : IRequest<bool>;

public class ApprovePaymentVoucherCommandHandler : IRequestHandler<ApprovePaymentVoucherCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkflowService _workflowService;

    public ApprovePaymentVoucherCommandHandler(IUnitOfWork unitOfWork, IWorkflowService workflowService)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
    }

    public async Task<bool> Handle(ApprovePaymentVoucherCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var voucher = await _unitOfWork.Repository<PaymentVoucher>().GetByIdAsync(request.VoucherId);
            if (voucher == null || voucher.Status == VoucherStatus.Approved)
                return false;

            var isFullyApproved = await _workflowService.IsFullyApprovedAsync(WorkflowDocumentType.PaymentVoucher, voucher.Id, cancellationToken);
            if (!isFullyApproved)
                throw new AccountingDomainException("Payment voucher is not fully approved in the workflow.");

            voucher.Status = VoucherStatus.Approved;
            _unitOfWork.Repository<PaymentVoucher>().Update(voucher);

            // Load allocations
            var allocations = await _unitOfWork.Repository<InvoicePayment>().FindAsync(a => a.PaymentVoucherId == voucher.Id);
            voucher.InvoicePayments = allocations.ToList();

            // AP Settlement: Reduce Purchase Invoice Remaining Amount
            if (voucher.PurchaseInvoiceId.HasValue && !voucher.InvoicePayments.Any())
            {
                var invoice = await _unitOfWork.Repository<PurchaseInvoice>().GetByIdAsync(voucher.PurchaseInvoiceId.Value);
                ValidatePayableInvoice(invoice, voucher.Amount);

                invoice!.PaidAmount += voucher.Amount;
                invoice.RemainingAmount -= voucher.Amount;
                _unitOfWork.Repository<PurchaseInvoice>().Update(invoice);

                var invoicePayment = new InvoicePayment
                {
                    PaymentVoucherId = voucher.Id,
                    PurchaseInvoiceId = invoice.Id,
                    Amount = voucher.Amount,
                    SettlementDate = DateTime.UtcNow
                };
                await _unitOfWork.Repository<InvoicePayment>().AddAsync(invoicePayment);
            }
            else
            {
                foreach (var allocation in voucher.InvoicePayments)
                {
                    if (allocation.PurchaseInvoiceId.HasValue)
                    {
                        var invoice = await _unitOfWork.Repository<PurchaseInvoice>().GetByIdAsync(allocation.PurchaseInvoiceId.Value);
                        ValidatePayableInvoice(invoice, allocation.Amount);

                        invoice!.PaidAmount += allocation.Amount;
                        invoice.RemainingAmount -= allocation.Amount;
                        _unitOfWork.Repository<PurchaseInvoice>().Update(invoice);
                    }
                }
            }

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static void ValidatePayableInvoice(PurchaseInvoice? invoice, decimal paymentAmount)
    {
        if (paymentAmount <= 0)
            throw new AccountingDomainException("Payment amount must be greater than zero.");

        if (invoice is null)
            throw new PurchasingDomainException("Purchase invoice was not found.");

        if (invoice.Status != PurchaseInvoiceStatus.Approved &&
            invoice.Status != PurchaseInvoiceStatus.ApprovedForPayment)
            throw new PurchasingDomainException("Only approved or payment-ready purchase invoices can be paid.");

        if (paymentAmount > invoice.RemainingAmount)
            throw new AccountingDomainException("Payment amount cannot exceed the purchase invoice remaining amount.");
    }
}
