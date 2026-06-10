using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Treasury.ReceiptVouchers.Commands.ApproveReceiptVoucher;

public record ApproveReceiptVoucherCommand(Guid VoucherId) : IRequest<bool>;

public class ApproveReceiptVoucherCommandHandler(
    IUnitOfWork unitOfWork,
    IAccountingPostingService accountingPostingService)
    : IRequestHandler<ApproveReceiptVoucherCommand, bool>
{
    public async Task<bool> Handle(ApproveReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            var voucher = await unitOfWork.Repository<ReceiptVoucher>().Query()
                .Include(v => v.InvoicePayments)
                .FirstOrDefaultAsync(v => v.Id == request.VoucherId, cancellationToken);
            
            if (voucher == null) return false;
            if (voucher.Status == VoucherStatus.Approved) return true;

            voucher.Status = VoucherStatus.Approved;
            unitOfWork.Repository<ReceiptVoucher>().Update(voucher);

            // 1. Post to Accounting
            var journalEntry = await accountingPostingService.PostReceiptVoucherAsync(voucher, cancellationToken);
            voucher.JournalEntryId = journalEntry.Id;
            await unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);

            // 2. Apply Payment to Sales Invoices (Multi-allocation support)
            // Rule: Each InvoicePayment represents a settlement against a specific invoice
            if (voucher.InvoicePayments != null && voucher.InvoicePayments.Any())
            {
                foreach (var payment in voucher.InvoicePayments)
                {
                    if (payment.SalesInvoiceId.HasValue)
                    {
                        var invoice = await unitOfWork.Repository<SalesInvoice>().GetByIdAsync(payment.SalesInvoiceId.Value);
                        if (invoice != null)
                        {
                            invoice.ApplyPayment(payment.AllocatedAmount); // Use domain method
                            unitOfWork.Repository<SalesInvoice>().Update(invoice);
                        }
                    }
                }
            }
            // Backward compatibility for single invoice scenario if InvoicePayments is empty
            else if (voucher.SalesInvoiceId.HasValue)
            {
                var invoice = await unitOfWork.Repository<SalesInvoice>().GetByIdAsync(voucher.SalesInvoiceId.Value);
                if (invoice != null)
                {
                    invoice.ApplyPayment(voucher.Amount);
                    unitOfWork.Repository<SalesInvoice>().Update(invoice);

                    // Create the missing InvoicePayment record for traceability
                    var invoicePayment = InvoicePayment.Create(
                        salesInvoiceId: invoice.Id,
                        receiptVoucherId: voucher.Id,
                        allocatedAmount: voucher.Amount,
                        settlementDate: DateTime.UtcNow,
                        tenantId: voucher.TenantId
                    );
                    await unitOfWork.Repository<InvoicePayment>().AddAsync(invoicePayment);
                }
            }

            await unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
