using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Procurement;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Commands.CreatePurchaseReturn;

// ═══════════════════════════════════════════════════════════
// Command
// ═══════════════════════════════════════════════════════════

public record CreatePurchaseReturnCommand : IRequest<Guid>
{
    public Guid CompanyId { get; init; }
    public Guid GoodsReceiptNoteId { get; init; }
    public DateTime ReturnDate { get; init; }
    public string Reason { get; init; } = default!;
    public List<PurchaseReturnLineDto> Lines { get; init; } = new();
}

public record PurchaseReturnLineDto
{
    public Guid GRNLineId { get; init; }
    public Guid ItemId { get; init; }
    public decimal ReturnQuantity { get; init; }
    public decimal UnitCost { get; init; }
}

// ═══════════════════════════════════════════════════════════
// Validator
// ═══════════════════════════════════════════════════════════

public class CreatePurchaseReturnCommandValidator : AbstractValidator<CreatePurchaseReturnCommand>
{
    public CreatePurchaseReturnCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.GoodsReceiptNoteId).NotEmpty();
        RuleFor(x => x.ReturnDate).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("Purchase return must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.GRNLineId).NotEmpty();
            line.RuleFor(l => l.ItemId).NotEmpty();
            line.RuleFor(l => l.ReturnQuantity).GreaterThan(0);
            line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

// ═══════════════════════════════════════════════════════════
// Handler
// ═══════════════════════════════════════════════════════════

public class CreatePurchaseReturnCommandHandler : IRequestHandler<CreatePurchaseReturnCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;
    private readonly IAccountMappingService _accountMappingService;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IPeriodClosingService _periodService;

    public CreatePurchaseReturnCommandHandler(
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

    public async Task<Guid> Handle(CreatePurchaseReturnCommand command, CancellationToken cancellationToken)
    {
        // 1. فحص الفترة المحاسبية — لا قيد في فترة مغلقة
        await _periodService.ValidateOpenAsync(command.ReturnDate, cancellationToken);

        // 2. جلب GRN والتحقق من صحتها
        var grn = await _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == command.GoodsReceiptNoteId && g.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"GRN {command.GoodsReceiptNoteId} not found.");

        if (grn.Status != GRNStatus.Approved)
            throw new InvalidOperationException($"Can only return goods from Approved GRN. Current status: {grn.Status}");

        // 3. التحقق من كميات المردود مقابل GRN
        foreach (var line in command.Lines)
        {
            var grnLine = grn.Lines.FirstOrDefault(l => l.Id == line.GRNLineId)
                ?? throw new InvalidOperationException($"GRN Line {line.GRNLineId} not found in GRN {grn.Id}.");

            if (line.ReturnQuantity > grnLine.ReceivedQuantity)
                throw new InvalidOperationException(
                    $"Return quantity {line.ReturnQuantity} exceeds received quantity {grnLine.ReceivedQuantity} for item {line.ItemId}.");
        }

        // 4. توليد رقم تسلسلي
        var returnNumber = await _numberingService.GenerateAsync("PRET", command.CompanyId, Guid.Empty, cancellationToken);

        // 5. إنشاء مردود المشتريات
        var purchaseReturn = PurchaseReturn.Create(
            command.CompanyId,
            command.GoodsReceiptNoteId,
            grn.PurchaseOrderId,
            grn.SupplierId,
            grn.WarehouseId,
            command.ReturnDate,
            returnNumber,
            command.Reason);

        foreach (var line in command.Lines)
        {
            purchaseReturn.AddLine(line.GRNLineId, line.ItemId, line.ReturnQuantity, line.UnitCost);
        }

        // 6. اعتماد المردود مباشرة
        purchaseReturn.Approve(command.CompanyId); // سيُطلق PurchaseReturnApprovedEvent

        // 7. ترحيل القيد المحاسبي العكسي
        // عكس قيد GRNI: Dr: AP Accrued Liability / Cr: GRNI Accrual
        var totalReturnValue = purchaseReturn.TotalReturnValue;

        var grniAccrualAccount = await _accountMappingService.GetAccountCodeAsync(
            AccountMappingKey.GRNI, command.CompanyId, cancellationToken);
        var apAccruedLiabilityAccount = await _accountMappingService.GetAccountCodeAsync(
            AccountMappingKey.AccountsPayable, command.CompanyId, cancellationToken);

        var period = await _context.AccountingPeriods.FirstOrDefaultAsync(p => p.CompanyId == command.CompanyId && p.StartDate <= command.ReturnDate && p.EndDate >= command.ReturnDate, cancellationToken);
        var periodId = period?.Id ?? Guid.Empty;

        await _accountingPostingService.PostAsync(new PostJournalRequest(
            command.CompanyId,
            periodId,
            $"Purchase Return: {command.Reason}",
            returnNumber,
            command.ReturnDate,
            new List<JournalLineRequest>
            {
                new JournalLineRequest(apAccruedLiabilityAccount, totalReturnValue, 0, $"Purchase Return — AP Reversal" ),
                new JournalLineRequest(grniAccrualAccount, 0, totalReturnValue, $"Purchase Return — GRNI Reversal" )
            }
        ), cancellationToken);

        // 8. إنشاء إشعار مدين
        var debitNoteNumber = await _numberingService.GenerateAsync("DN", command.CompanyId, Guid.Empty, cancellationToken);
        var debitNote = DebitNote.Create(
            purchaseReturn.Id,
            grn.SupplierId,
            totalReturnValue,
            debitNoteNumber);

        _context.PurchaseReturns.Add(purchaseReturn);
        _context.DebitNotes.Add(debitNote);
        await _context.SaveChangesAsync(cancellationToken);

        return purchaseReturn.Id;
    }
}
