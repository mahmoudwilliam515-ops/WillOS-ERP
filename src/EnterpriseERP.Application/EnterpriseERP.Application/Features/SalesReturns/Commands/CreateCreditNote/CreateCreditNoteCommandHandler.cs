using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Application.Common.Interfaces.Services;
using FluentValidation;
using EnterpriseERP.Application.Common.Interfaces.Services;
using MediatR;
using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using EnterpriseERP.Application.Common.Interfaces.Services;

namespace EnterpriseERP.Application.Features.SalesReturns.Commands.CreateCreditNote;

// ═══════════════════════════════════════════════════════════
// Command
// ═══════════════════════════════════════════════════════════

public record CreateCreditNoteCommand : IRequest<Guid>
{
    public Guid CompanyId { get; init; }
    public Guid OriginalInvoiceId { get; init; }
    public DateTime ReturnDate { get; init; }
    public string Reason { get; init; } = default!;
    public List<SalesReturnLineDto> Lines { get; init; } = new();
}

public record SalesReturnLineDto
{
    public Guid OriginalInvoiceLineId { get; init; }
    public Guid ItemId { get; init; }
    public decimal ReturnQuantity { get; init; }
    public decimal UnitPrice { get; init; }
}

// ═══════════════════════════════════════════════════════════
// Validator
// ═══════════════════════════════════════════════════════════

public class CreateCreditNoteCommandValidator : AbstractValidator<CreateCreditNoteCommand>
{
    public CreateCreditNoteCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.OriginalInvoiceId).NotEmpty();
        RuleFor(x => x.ReturnDate).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Credit note must have at least one return line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId).NotEmpty();
            line.RuleFor(l => l.ReturnQuantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

// ═══════════════════════════════════════════════════════════
// Handler
// ═══════════════════════════════════════════════════════════

public class CreateCreditNoteCommandHandler : IRequestHandler<CreateCreditNoteCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;
    private readonly IAccountMappingService _accountMappingService;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IPeriodClosingService _periodService;

    public CreateCreditNoteCommandHandler(
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

    public async Task<Guid> Handle(CreateCreditNoteCommand command, CancellationToken cancellationToken)
    {
        // 1. فحص الفترة المحاسبية
        await _periodService.ValidateOpenAsync(command.ReturnDate, cancellationToken);

        // 2. جلب الفاتورة الأصلية
        var originalInvoice = await _context.SalesInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == command.OriginalInvoiceId && i.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Sales Invoice {command.OriginalInvoiceId} not found.");

        // 3. توليد رقم تسلسلي
        var creditNoteNumber = await _numberingService.GenerateAsync("CN", command.CompanyId, Guid.Empty, cancellationToken);

        // 4. حساب القيمة الإجمالية
        decimal totalReturnAmount = command.Lines.Sum(l => l.ReturnQuantity * l.UnitPrice);

        // 5. إنشاء مردود المبيعات
        var salesReturn = SalesReturn.Create(
            command.CompanyId,
            command.OriginalInvoiceId,
            originalInvoice.CustomerId,
            command.ReturnDate,
            creditNoteNumber,
            command.Reason);

        foreach (var line in command.Lines)
        {
            salesReturn.AddLine(line.ItemId, line.ReturnQuantity, line.UnitPrice);
        }

        // 6. ترحيل القيود المحاسبية العكسية
        // عكس قيد AR: Dr: Sales Revenue / Cr: Accounts Receivable
        // عكس COGS:   Dr: Inventory / Cr: Cost of Goods Sold

        var arAccount = await _accountMappingService.GetAccountCodeAsync(
            AccountMappingKey.AccountsReceivable, command.CompanyId, cancellationToken);
        var salesRevenueAccount = await _accountMappingService.GetAccountCodeAsync(
            AccountMappingKey.SalesReturnExpense, command.CompanyId, cancellationToken); // Or SalesRevenue if you want

        var period = await _context.AccountingPeriods.FirstOrDefaultAsync(p => p.CompanyId == command.CompanyId && p.StartDate <= command.ReturnDate && p.EndDate >= command.ReturnDate, cancellationToken);
        var periodId = period?.Id ?? Guid.Empty;

        // قيد عكس الإيراد (Credit Note)
        await _accountingPostingService.PostAsync(new PostJournalRequest(
            command.CompanyId,
            periodId,
            $"Credit Note: {command.Reason}",
            creditNoteNumber,
            command.ReturnDate,
            new List<JournalLineRequest>
            {
                new JournalLineRequest(salesRevenueAccount, totalReturnAmount, 0, "Sales Return — Revenue Reversal"),
                new JournalLineRequest(arAccount, 0, totalReturnAmount, "Sales Return — AR Reduction")
            }
        ), cancellationToken);

        // 7. تقليل الرصيد المتبقي في الفاتورة الأصلية
        originalInvoice.ApplyCreditNote(salesReturn.Id, totalReturnAmount);

        _context.SalesReturns.Add(salesReturn);
        await _context.SaveChangesAsync(cancellationToken);

        return salesReturn.Id;
    }
}
