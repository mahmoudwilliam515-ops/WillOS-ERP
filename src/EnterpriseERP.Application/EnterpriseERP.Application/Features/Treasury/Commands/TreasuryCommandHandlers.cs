using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Treasury;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;


namespace EnterpriseERP.Application.Features.Treasury.Commands;

// ═══════════════════════════════════════════════════════════════
// CreateReceiptVoucher — استلام دفعة من عميل + تسوية AR
// ═══════════════════════════════════════════════════════════════

public record CreateReceiptVoucherCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid BankAccountId { get; init; }
    public DateTime ReceiptDate { get; init; }
    public decimal Amount { get; init; }
    public string? Reference { get; init; }
    public string? Notes { get; init; }
    public List<InvoiceAllocationDto> InvoiceAllocations { get; init; } = new();
}

public record InvoiceAllocationDto
{
    public Guid InvoiceId { get; init; }
    public decimal AllocatedAmount { get; init; }
}

public class CreateReceiptVoucherCommandValidator : AbstractValidator<CreateReceiptVoucherCommand>
{
    public CreateReceiptVoucherCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
        RuleFor(x => x.ReceiptDate).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Receipt amount must be positive");
        RuleFor(x => x.InvoiceAllocations)
            .Must((cmd, allocs) => allocs.Sum(a => a.AllocatedAmount) <= cmd.Amount)
            .WithMessage("Total allocated amount cannot exceed receipt amount");
    }
}

public class CreateReceiptVoucherCommandHandler : IRequestHandler<CreateReceiptVoucherCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;
    private readonly IAccountMappingService _accountMappingService;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IPeriodClosingService _periodService;

    public CreateReceiptVoucherCommandHandler(
        IAppDbContext context,
        IOrderNumberGenerator numberingService,
        IAccountMappingService accountMappingService,
        IAccountingPostingService accountingPostingService,
        IPeriodClosingService periodService)
    {
        _context = context;
        _numberingService = numberingService;
        _accountMappingService = accountMappingService;
        _accountingPostingService = accountingPostingService;
        _periodService = periodService;
    }

    public async Task<Result<Guid>> Handle(CreateReceiptVoucherCommand command, CancellationToken cancellationToken)
    {
        // 1. فحص الفترة المحاسبية
        var isPeriodOpen = await _periodService.IsOpenAsync(command.ReceiptDate, cancellationToken);
        if (!isPeriodOpen)
            return Result.Failure<Guid>(new Error("Accounting.PeriodClosed", "Accounting period is closed for the selected date."));

        // 2. التحقق من الحساب البنكي
        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == command.BankAccountId && b.CompanyId == command.CompanyId, cancellationToken);

        if (bankAccount == null)
            return Result.Failure<Guid>(new Error("Treasury.BankAccountNotFound", $"Bank account {command.BankAccountId} not found"));

        // 3. AR Settlement — تسوية كل فاتورة مخصصة
        var totalAllocated = 0m;
        foreach (var allocation in command.InvoiceAllocations)
        {
            var invoice = await _context.SalesInvoices
                .FirstOrDefaultAsync(i => i.Id == allocation.InvoiceId && i.CompanyId == command.CompanyId, cancellationToken);

            if (invoice == null)
                return Result.Failure<Guid>(new Error("Sales.InvoiceNotFound", $"Invoice {allocation.InvoiceId} not found"));

            if (allocation.AllocatedAmount > invoice.RemainingAmount)
                return Result.Failure<Guid>(new Error("Sales.InvalidAllocation", 
                    $"Allocated amount {allocation.AllocatedAmount} exceeds remaining balance {invoice.RemainingAmount} on invoice {invoice.InvoiceNumber}"));

            invoice.ApplyPayment(allocation.AllocatedAmount);
            totalAllocated += allocation.AllocatedAmount;
        }

        // 4. إنشاء إيصال القبض
        var voucherNumber = await _numberingService.GenerateAsync("RV", command.CompanyId, Guid.Empty, cancellationToken);
        var voucher = new ReceiptVoucher 
        {
            CompanyId = command.CompanyId,
            CustomerId = command.CustomerId,
            BankAccountId = command.BankAccountId,
            ReceiptDate = command.ReceiptDate,
            VoucherDate = command.ReceiptDate, // Same as receipt date
            Amount = command.Amount,
            VoucherNumber = voucherNumber,
            Reference = command.Reference,
            Notes = command.Notes ?? string.Empty,
            Status = VoucherStatus.Draft
        };

        // 5. القيد المحاسبي
        var postingResult = await PostReceiptAccountingAsync(voucher, totalAllocated, command.CompanyId, cancellationToken);
        if (!postingResult.IsSuccess)
            return Result.Failure<Guid>(postingResult.Error);

        _context.ReceiptVouchers.Add(voucher);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(voucher.Id);
    }

    private async Task<Result> PostReceiptAccountingAsync(
        ReceiptVoucher voucher,
        decimal allocatedToAR,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        try 
        {
            var bankGLAccount = await _accountMappingService
                .GetBankAccountGLCodeAsync(voucher.BankAccountId.Value, companyId, cancellationToken);
            var arControlAccount = await _accountMappingService
                .GetAccountCodeAsync(AccountMappingKey.AccountsReceivable, companyId, cancellationToken);
            var unappliedCashAccount = await _accountMappingService
                .GetAccountCodeAsync(AccountMappingKey.CashAndBank, companyId, cancellationToken); // Or add UnappliedCash to enum

            var period = await _context.AccountingPeriods.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.StartDate <= voucher.ReceiptDate && p.EndDate >= voucher.ReceiptDate, cancellationToken);
            var periodId = period?.Id ?? Guid.Empty;

            var journalLines = new List<JournalLineRequest>
            {
                // Dr: Bank Account — زيادة النقدية
                new JournalLineRequest(bankGLAccount, voucher.Amount, 0, "Receipt from customer")
            };

            if (allocatedToAR > 0)
            {
                // Cr: AR Control — تخفيض ذمم العميل
                journalLines.Add(new JournalLineRequest(arControlAccount, 0, allocatedToAR, "AR Settlement"));
            }

            // الجزء غير المخصص لفاتورة محددة → حساب نقدية معلقة
            var unallocated = voucher.Amount - allocatedToAR;
            if (unallocated > 0.001m)
            {
                journalLines.Add(new JournalLineRequest(unappliedCashAccount, 0, unallocated, "Unapplied Cash"));
            }

            await _accountingPostingService.PostAsync(new PostJournalRequest(
                companyId,
                periodId,
                $"Receipt from customer {voucher.CustomerId}. Ref: {voucher.Reference}",
                $"RV-{voucher.VoucherNumber}",
                voucher.ReceiptDate,
                journalLines
            ), cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Accounting.PostingError", ex.Message));
        }
    }
}

// ═══════════════════════════════════════════════════════════════
// CreatePaymentVoucher — دفع لمورد + تسوية AP
// ═══════════════════════════════════════════════════════════════

public record CreatePaymentVoucherCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; init; }
    public Guid SupplierId { get; init; }
    public Guid BankAccountId { get; init; }
    public DateTime PaymentDate { get; init; }
    public decimal Amount { get; init; }
    public string? Reference { get; init; }
    public string? Notes { get; init; }
    public List<APInvoiceAllocationDto> InvoiceAllocations { get; init; } = new();
}

public record APInvoiceAllocationDto
{
    public Guid InvoiceId { get; init; }
    public decimal AllocatedAmount { get; init; }
}

public class CreatePaymentVoucherCommandHandler : IRequestHandler<CreatePaymentVoucherCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;
    private readonly IAccountMappingService _accountMappingService;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IPeriodClosingService _periodService;

    public CreatePaymentVoucherCommandHandler(
        IAppDbContext context,
        IOrderNumberGenerator numberingService,
        IAccountMappingService accountMappingService,
        IAccountingPostingService accountingPostingService,
        IPeriodClosingService periodService)
    {
        _context = context;
        _numberingService = numberingService;
        _accountMappingService = accountMappingService;
        _accountingPostingService = accountingPostingService;
        _periodService = periodService;
    }

    public async Task<Result<Guid>> Handle(CreatePaymentVoucherCommand command, CancellationToken cancellationToken)
    {
        // 1. فحص الفترة
        var isPeriodOpen = await _periodService.IsOpenAsync(command.PaymentDate, cancellationToken);
        if (!isPeriodOpen)
            return Result.Failure<Guid>(new Error("Accounting.PeriodClosed", "Accounting period is closed for the selected date."));

        // 2. التحقق من حساب البنك والرصيد الكافي
        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == command.BankAccountId && b.CompanyId == command.CompanyId, cancellationToken);

        if (bankAccount == null)
            return Result.Failure<Guid>(new Error("Treasury.BankAccountNotFound", $"Bank account {command.BankAccountId} not found"));

        if (bankAccount.CurrentBalance < command.Amount)
            return Result.Failure<Guid>(new Error("Treasury.InsufficientBalance", 
                $"Insufficient bank balance. Available: {bankAccount.CurrentBalance:N2}, Required: {command.Amount:N2}"));

        // 3. Payment Block — فحص Match Status لكل فاتورة
        // القاعدة الذهبية: لا دفع لفاتورة غير مطابقة
        foreach (var allocation in command.InvoiceAllocations)
        {
            var invoice = await _context.PurchaseInvoices
                .FirstOrDefaultAsync(i => i.Id == allocation.InvoiceId && i.CompanyId == command.CompanyId, cancellationToken);

            if (invoice == null)
                return Result.Failure<Guid>(new Error("Purchasing.InvoiceNotFound", $"Purchase Invoice {allocation.InvoiceId} not found"));

            if (invoice.MatchStatus == InvoiceMatchStatus.Unmatched)
                return Result.Failure<Guid>(new Error("Purchasing.PaymentBlocked", 
                    $"Invoice {invoice.InvoiceNumber} has not passed 3-Way Match. Payment is blocked."));

            // Removed pending check as InvoiceMatchStatus does not have Pending

            if (allocation.AllocatedAmount > invoice.RemainingAmount)
                return Result.Failure<Guid>(new Error("Purchasing.InvalidAllocation", 
                    $"Allocated {allocation.AllocatedAmount} exceeds remaining AP balance {invoice.RemainingAmount} on invoice {invoice.InvoiceNumber}"));

            invoice.ApplyPayment(allocation.AllocatedAmount);
        }

        // 4. إنشاء سند الدفع
        var voucherNumber = await _numberingService.GenerateAsync("PV", command.CompanyId, Guid.Empty, cancellationToken);
        var voucher = new PaymentVoucher 
        {
            CompanyId = command.CompanyId,
            SupplierId = command.SupplierId,
            BankAccountId = command.BankAccountId,
            PaymentDate = command.PaymentDate,
            VoucherDate = command.PaymentDate, // Same as payment date
            Amount = command.Amount,
            VoucherNumber = voucherNumber,
            Reference = command.Reference,
            Notes = command.Notes ?? string.Empty,
            Status = VoucherStatus.Draft
        };

        // 5. تخفيض رصيد البنك
        bankAccount.Debit(command.Amount);

        // 6. القيد المحاسبي
        var postingResult = await PostPaymentAccountingAsync(voucher, command.CompanyId, cancellationToken);
        if (!postingResult.IsSuccess)
            return Result.Failure<Guid>(postingResult.Error);

        _context.PaymentVouchers.Add(voucher);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(voucher.Id);
    }

    private async Task<Result> PostPaymentAccountingAsync(
        PaymentVoucher voucher,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        try 
        {
            var bankGLAccount = await _accountMappingService
                .GetBankAccountGLCodeAsync(voucher.BankAccountId.Value, companyId, cancellationToken);
            var apControlAccount = await _accountMappingService
                .GetAccountCodeAsync(AccountMappingKey.AccountsPayable, companyId, cancellationToken);

            var period = await _context.AccountingPeriods.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.StartDate <= voucher.PaymentDate && p.EndDate >= voucher.PaymentDate, cancellationToken);
            var periodId = period?.Id ?? Guid.Empty;

            await _accountingPostingService.PostAsync(new PostJournalRequest(
                companyId,
                periodId,
                $"Payment to supplier {voucher.SupplierId}. Ref: {voucher.Reference}",
                $"PV-{voucher.VoucherNumber}",
                voucher.PaymentDate,
                new List<JournalLineRequest>
                {
                    // Dr: AP Control — تخفيض الالتزامات
                    new JournalLineRequest(apControlAccount, voucher.Amount, 0, "AP Settlement"),
                    // Cr: Bank Account — تخفيض النقدية
                    new JournalLineRequest(bankGLAccount, 0, voucher.Amount, "Payment from Bank")
                }
            ), cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Accounting.PostingError", ex.Message));
        }
    }
}
