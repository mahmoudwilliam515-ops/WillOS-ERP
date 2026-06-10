// ============================================================
// P0-Task-4: Inventory Guard مع Concurrency قبل موافقة الفاتورة
// الملف: src/Application/Features/SalesInvoices/Commands/
//        ApproveSalesInvoice/ApproveSalesInvoiceCommandHandler.cs
// ============================================================
// القاعدة: لا موافقة على فاتورة إذا المخزون غير كافٍ
// القاعدة: RowVersion لمنع Race Condition
// ============================================================

using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EnterpriseERP.Application.Features.SalesInvoices.Commands.ApproveSalesInvoice;

public class ApproveSalesInvoiceCommandHandler
    : IRequestHandler<ApproveSalesInvoiceCommand, ApproveSalesInvoiceResult>
{
    private readonly IAppDbContext _context;
    private readonly IAccountingPostingService _accountingService;

    public ApproveSalesInvoiceCommandHandler(
        IAppDbContext context,
        IAccountingPostingService accountingService)
    {
        _context = context;
        _accountingService = accountingService;
    }

    public async Task<ApproveSalesInvoiceResult> Handle(
        ApproveSalesInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. جلب الفاتورة مع بنودها ──────────────────────────

        var invoice = await _context.SalesInvoices
            .Include(i => i.Lines)
            .Where(i => i.Id == request.InvoiceId
                     && i.TenantId == request.TenantId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Sales Invoice {request.InvoiceId} not found.");

        // ── 2. تحقق من الحالة ──────────────────────────────────

        if (invoice.Status != InvoiceStatus.Draft)
            throw new SalesDomainException(
                $"لا يمكن الموافقة على فاتورة بحالة: {invoice.Status}. " +
                $"يُقبل فقط Draft.");

        // ── 3. تحقق من الفترة المحاسبية ────────────────────────

        var period = await _context.AccountingPeriods
            .Include(p => p.FiscalYear)
            .Where(p => p.FiscalYear.TenantId == request.TenantId
                     && p.StartDate <= invoice.InvoiceDate
                     && p.EndDate >= invoice.InvoiceDate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new SalesDomainException(
                $"لا توجد فترة محاسبية لتاريخ الفاتورة: {invoice.InvoiceDate:yyyy-MM-dd}");

        if (period.Status == AccountingPeriodStatus.Closed)
            throw new SalesDomainException(
                $"الفترة المحاسبية ({period.PeriodName}) مغلقة. " +
                $"لا يمكن ترحيل فاتورة في فترة مغلقة.");

        // ── 4. 🔴 INVENTORY GUARD — التحقق من توفر المخزون ──────
        // هذا هو الحارس الجديد الذي يُضاف في Sprint 1

        await ValidateInventoryAvailabilityAsync(invoice, request.TenantId, cancellationToken);

        // ── 5. تحديث الفاتورة ───────────────────────────────────

        invoice.Approve(request.ApprovedBy, DateTime.UtcNow);

        // ── 6. تخفيض المخزون مع Concurrency Protection ─────────

        await DeductInventoryWithConcurrencyAsync(invoice, request.TenantId, cancellationToken);

        // ── 7. تهيئة AR Settlement ──────────────────────────────

        invoice.InitializeSettlement();

        // ── 8. Accounting Posting ───────────────────────────────

        await _accountingService.PostSalesInvoiceAsync(invoice, cancellationToken);

        // ── 9. حفظ ─────────────────────────────────────────────

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Race Condition: مخزون تغيَّر أثناء المعالجة
            throw new SalesDomainException(
                "حدث تعارض في المخزون أثناء الموافقة على الفاتورة. " +
                "الرجاء إعادة المحاولة. إذا استمرت المشكلة، تحقق من توفر المخزون.",
                ex);
        }

        return new ApproveSalesInvoiceResult(
            InvoiceId: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            ApprovedAt: DateTime.UtcNow);
    }

    // ── Private Methods ───────────────────────────────────────

    /// <summary>
    /// التحقق من توفر كميات كافية لكل بند في الفاتورة
    /// يجمع الكميات لنفس المنتج في نفس المستودع قبل الفحص
    /// </summary>
    private async Task ValidateInventoryAvailabilityAsync(
        SalesInvoice invoice,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        // تجميع الكميات المطلوبة حسب (ItemId, WarehouseId)
        var requiredQuantities = invoice.Lines
            .Where(l => l.ItemId != Guid.Empty) // Guid is not nullable here, check for Empty
            .GroupBy(l => new { l.ItemId, l.WarehouseId })
            .Select(g => new
            {
                g.Key.ItemId,
                g.Key.WarehouseId,
                RequiredQty = g.Sum(l => l.Quantity)
            })
            .ToList();

        if (!requiredQuantities.Any())
            return; // فاتورة خدمات فقط — لا مخزون مطلوب

        var insufficientItems = new List<string>();

        foreach (var req in requiredQuantities)
        {
            var stock = await _context.InventoryBalances
                .Where(ib => ib.ItemId == req.ItemId
                          && ib.WarehouseId == req.WarehouseId
                          && ib.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken);

            var availableQty = stock?.AvailableQuantity ?? 0;

            if (availableQty < req.RequiredQty)
            {
                // جلب اسم المنتج للرسالة
                var itemName = await _context.Items
                    .Where(i => i.Id == req.ItemId && i.TenantId == tenantId)
                    .Select(i => i.Name)
                    .FirstOrDefaultAsync(cancellationToken) ?? req.ItemId.ToString();

                insufficientItems.Add(
                    $"• {itemName}: مطلوب {req.RequiredQty:N2}، " +
                    $"متاح {availableQty:N2}، " +
                    $"عجز {(req.RequiredQty - availableQty):N2}");
            }
        }

        if (insufficientItems.Any())
        {
            throw new InsufficientInventoryException(
                $"لا يمكن الموافقة على الفاتورة {invoice.InvoiceNumber} — " +
                $"المخزون غير كافٍ للبنود التالية:\n" +
                string.Join("\n", insufficientItems));
        }
    }

    /// <summary>
    /// تخفيض المخزون مع Optimistic Concurrency (RowVersion)
    /// </summary>
    private async Task DeductInventoryWithConcurrencyAsync(
        SalesInvoice invoice,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var requiredQuantities = invoice.Lines
            .Where(l => l.ItemId != Guid.Empty)
            .GroupBy(l => new { l.ItemId, l.WarehouseId })
            .Select(g => new
            {
                g.Key.ItemId,
                g.Key.WarehouseId,
                RequiredQty = g.Sum(l => l.Quantity)
            });

        foreach (var req in requiredQuantities)
        {
            var stock = await _context.InventoryBalances
                .Where(ib => ib.ItemId == req.ItemId
                          && ib.WarehouseId == req.WarehouseId
                          && ib.TenantId == tenantId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InventoryDomainException(
                    $"لا يوجد سجل مخزون للمنتج {req.ItemId} " +
                    $"في المستودع {req.WarehouseId}");

            // تخفيض الكمية المتاحة
            // RowVersion موجود في InventoryBalance → EF Core يرصد التعارض تلقائياً
            stock.Deduct(req.RequiredQty, invoice.Id, invoice.InvoiceDate);
        }
    }
}
