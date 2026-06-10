using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.CreateGRN;

// ─── Command ─────────────────────────────────────────────────────

public record CreateGRNCommand : IRequest<Result<Guid>>
{
    public Guid PurchaseOrderId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid WarehouseId { get; init; }
    public DateTime ReceiptDate { get; init; }
    public string? DeliveryNoteReference { get; init; }
    public string? Notes { get; init; }
    public List<CreateGRNLineDto> Lines { get; init; } = new();
}

public record CreateGRNLineDto
{
    public Guid PurchaseOrderLineId { get; init; }
    public Guid ItemId { get; init; }
    public decimal ReceivedQuantity { get; init; }
    public decimal UnitCost { get; init; }
    public string? BatchNumber { get; init; }
}

// ─── Validator ───────────────────────────────────────────────────

public class CreateGRNCommandValidator : AbstractValidator<CreateGRNCommand>
{
    public CreateGRNCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ReceiptDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
        RuleFor(x => x.Lines).NotEmpty().WithMessage("GRN must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.PurchaseOrderLineId).NotEmpty();
            line.RuleFor(l => l.ItemId).NotEmpty();
            line.RuleFor(l => l.ReceivedQuantity).GreaterThan(0);
            line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}

// ─── Handler ─────────────────────────────────────────────────────

public class CreateGRNCommandHandler : IRequestHandler<CreateGRNCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;
    private readonly IPeriodClosingService _periodService;

    public CreateGRNCommandHandler(
        IAppDbContext context,
        IOrderNumberGenerator numberingService,
        IPeriodClosingService periodService)
    {
        _context = context;
        _numberingService = numberingService;
        _periodService = periodService;
    }

    public async Task<Result<Guid>> Handle(CreateGRNCommand command, CancellationToken cancellationToken)
    {
        // 1. فحص أن فترة الاستلام مفتوحة
        var periodOpen = await _periodService.IsOpenAsync(command.ReceiptDate, cancellationToken);
        if (!periodOpen)
            return Result.Failure<Guid>(new Error("Accounting.PeriodClosed", "Accounting period is closed for the selected date."));

        // 2. التحقق من وجود أمر الشراء وحالته
        var po = await _context.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == command.PurchaseOrderId, cancellationToken);

        if (po == null)
            return Result.Failure<Guid>(new Error("Procurement.PONotFound", $"Purchase Order {command.PurchaseOrderId} not found."));

        if (po.Status != PurchaseOrderStatus.Approved && po.Status != PurchaseOrderStatus.PartiallyReceived)
            return Result.Failure<Guid>(new Error("Procurement.InvalidPOStatus", $"Purchase Order is in status {po.Status}. Only Approved or PartiallyReceived POs can have GRNs."));

        // 3. التحقق من أن الكميات لا تتجاوز ما تبقى في أمر الشراء
        foreach (var lineDto in command.Lines)
        {
            var poLine = po.Lines.FirstOrDefault(l => l.Id == lineDto.PurchaseOrderLineId);
            if (poLine == null)
                return Result.Failure<Guid>(new Error("Procurement.POLineNotFound", $"PO Line {lineDto.PurchaseOrderLineId} not found."));

            var alreadyReceivedQty = await _context.GoodsReceiptNoteLines
                .Where(l => l.PurchaseOrderLineId == lineDto.PurchaseOrderLineId)
                .Join(_context.GoodsReceiptNotes.Where(g => g.Status == GRNStatus.Approved),
                      l => l.GoodsReceiptNoteId, g => g.Id, (l, g) => l)
                .SumAsync(l => l.ReceivedQuantity, cancellationToken);

            var remainingQty = poLine.Quantity - alreadyReceivedQty;
            if (lineDto.ReceivedQuantity > remainingQty)
                return Result.Failure<Guid>(new Error("Procurement.QuantityExceeded",
                    $"Item {lineDto.ItemId}: Cannot receive {lineDto.ReceivedQuantity}. Only {remainingQty} remaining on PO line."));
        }

        // 4. توليد رقم GRN
        var grnNumber = await _numberingService.GenerateAsync("GRN", command.CompanyId, Guid.Empty, cancellationToken);

        // 5. إنشاء الكيان
        var grn = GoodsReceiptNote.Create(
            command.PurchaseOrderId,
            command.CompanyId,
            po.SupplierId,
            command.WarehouseId,
            command.ReceiptDate,
            grnNumber,
            command.DeliveryNoteReference,
            command.Notes);

        foreach (var line in command.Lines)
        {
            grn.AddLine(line.PurchaseOrderLineId, line.ItemId, line.ReceivedQuantity, line.UnitCost, line.BatchNumber);
        }

        _context.GoodsReceiptNotes.Add(grn);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(grn.Id);
    }
}
