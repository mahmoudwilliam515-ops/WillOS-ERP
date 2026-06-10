using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Treasury.Commands.SettleInvoicePayment;

public class SettleInvoicePaymentCommandHandler
    : IRequestHandler<SettleInvoicePaymentCommand, SettleInvoicePaymentResult>
{
    private readonly IAppDbContext _context;

    public SettleInvoicePaymentCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SettleInvoicePaymentResult> Handle(
        SettleInvoicePaymentCommand request,
        CancellationToken cancellationToken)
    {
        // 1. تحقق من وجود الفاتورة مع TenantId
        var invoice = await _context.SalesInvoices
            .Where(i => i.Id == request.SalesInvoiceId
                     && i.TenantId == request.TenantId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Sales Invoice {request.SalesInvoiceId} not found.");

        // 2. تحقق من وجود إيصال القبض مع TenantId
        var receiptVoucher = await _context.ReceiptVouchers
            .Where(rv => rv.Id == request.ReceiptVoucherId
                      && rv.TenantId == request.TenantId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Receipt Voucher {request.ReceiptVoucherId} not found.");

        // 3. تحقق من الفترة المحاسبية (لا تسوية في فترة مغلقة)
        var period = await _context.AccountingPeriods
            .Include(p => p.FiscalYear)
            .Where(p => p.FiscalYear.TenantId == request.TenantId
                     && p.StartDate <= request.SettlementDate
                     && p.EndDate >= request.SettlementDate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AccountingDomainException(
                $"لا توجد فترة محاسبية مفتوحة لتاريخ التسوية: {request.SettlementDate:yyyy-MM-dd}");

        if (period.Status == AccountingPeriodStatus.Closed)
            throw new AccountingDomainException(
                $"الفترة المحاسبية ({period.PeriodName}) مغلقة. " +
                $"لا يمكن تسجيل تسوية في فترة مغلقة.");

        // 4. تحقق من عدم وجود تسوية مزدوجة لنفس الزوج
        var existingSettlement = await _context.InvoicePayments
            .AnyAsync(ip => ip.ReceiptVoucherId == request.ReceiptVoucherId
                         && ip.SalesInvoiceId == request.SalesInvoiceId
                         && ip.TenantId == request.TenantId,
                      cancellationToken);

        if (existingSettlement)
            throw new AccountingDomainException(
                $"يوجد تسوية مسبقة لإيصال القبض {request.ReceiptVoucherId} " +
                $"مقابل الفاتورة {request.SalesInvoiceId}.");

        // 5. تطبيق الدفعة على الفاتورة (Domain Logic)
        invoice.ApplyPayment(request.AllocatedAmount);

        // 6. إنشاء كيان InvoicePayment
        var invoicePayment = InvoicePayment.Create(
            salesInvoiceId: request.SalesInvoiceId,
            receiptVoucherId: request.ReceiptVoucherId,
            allocatedAmount: request.AllocatedAmount,
            settlementDate: request.SettlementDate,
            tenantId: request.TenantId,
            notes: request.Notes);

        // 7. حفظ
        _context.InvoicePayments.Add(invoicePayment);
        await _context.SaveChangesAsync(cancellationToken);

        return new SettleInvoicePaymentResult(
            InvoicePaymentId: invoicePayment.Id,
            RemainingAmountAfter: invoice.RemainingAmount,
            NewStatus: invoice.SettlementStatus);
    }
}
