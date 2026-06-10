using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Commands.CreatePaymentVoucher;

public class CreatePaymentVoucherCommandHandler : IRequestHandler<CreatePaymentVoucherCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUserService;

    public CreatePaymentVoucherCommandHandler(
        IUnitOfWork unitOfWork,
        IWorkflowService workflowService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreatePaymentVoucherCommand request, CancellationToken cancellationToken)
    {
        if (request.CashAccountId == null && request.BankAccountId == null)
            throw new AccountingDomainException("Must specify either a Cash Account or a Bank Account.");

        if (request.Amount <= 0)
            throw new AccountingDomainException("Payment voucher amount must be greater than zero.");

        if (request.Allocations.Any())
        {
            var allocationTotal = request.Allocations.Sum(a => a.Amount);
            if (allocationTotal != request.Amount)
                throw new AccountingDomainException("Payment voucher allocations must equal the voucher amount.");

            foreach (var allocation in request.Allocations)
            {
                await ValidatePayableInvoiceAsync(allocation.InvoiceId, allocation.Amount);
            }
        }

        var voucherId = Guid.NewGuid();
        var voucher = new PaymentVoucher
        {
            Id = voucherId,
            VoucherNumber = $"PV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            VoucherDate = request.VoucherDate,
            SupplierId = request.SupplierId,
            CashAccountId = request.CashAccountId,
            BankAccountId = request.BankAccountId,
            Amount = request.Amount,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            Status = VoucherStatus.Draft,
            InvoicePayments = request.Allocations.Select(a => new InvoicePayment
            {
                PaymentVoucherId = voucherId,
                PurchaseInvoiceId = a.InvoiceId,
                Amount = a.Amount,
                SettlementDate = request.VoucherDate
            }).ToList()
        };

        await _unitOfWork.Repository<PaymentVoucher>().AddAsync(voucher);

        await _workflowService.SubmitForApprovalAsync(
            WorkflowDocumentType.PaymentVoucher,
            voucher.Id,
            voucher.VoucherNumber,
            voucher.Amount,
            _currentUserService.UserId ?? "System",
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return voucher.Id;
    }

    private async Task ValidatePayableInvoiceAsync(Guid invoiceId, decimal paymentAmount)
    {
        if (paymentAmount <= 0)
            throw new AccountingDomainException("Payment allocation amount must be greater than zero.");

        var invoice = await _unitOfWork.Repository<PurchaseInvoice>().GetByIdAsync(invoiceId);
        if (invoice is null)
            throw new PurchasingDomainException($"Purchase invoice with id {invoiceId} was not found.");

        if (invoice.Status != PurchaseInvoiceStatus.Matched && 
            invoice.Status != PurchaseInvoiceStatus.Approved && 
            invoice.Status != PurchaseInvoiceStatus.ApprovedForPayment)
        {
            throw new PurchasingDomainException(
                $"Invoice {invoice.InvoiceNumber} cannot be paid. " +
                $"Current status: {invoice.Status}. 3-way matching (Matched) is required.");
        }

        if (paymentAmount > invoice.RemainingAmount)
            throw new AccountingDomainException("Payment allocation cannot exceed the purchase invoice remaining amount.");
    }
}
