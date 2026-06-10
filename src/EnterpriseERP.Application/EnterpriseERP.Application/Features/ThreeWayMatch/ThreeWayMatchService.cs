using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.ThreeWayMatch;


public interface IThreeWayMatchService
{
    Task<MatchResult> ExecuteMatchAsync(Guid invoiceId, Guid companyId, CancellationToken cancellationToken);
}

/// <summary>
/// محرك المطابقة الثلاثية: PO vs GRN vs Invoice
/// Blueprint Section 1.1 — Three-Way Match Engine
/// 
/// القاعدة الذهبية: لا payment بدون Match ناجح
/// </summary>
public class ThreeWayMatchService : IThreeWayMatchService
{
    private readonly IAppDbContext _context;
    private readonly IAccountMappingService _accountMappingService;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IPeriodClosingService _periodService;

    public ThreeWayMatchService(
        IAppDbContext context,
        IAccountMappingService accountMappingService,
        IAccountingPostingService accountingPostingService,
        IPeriodClosingService periodService)
    {
        _context = context;
        _accountMappingService = accountMappingService;
        _accountingPostingService = accountingPostingService;
        _periodService = periodService;
    }

    /// <summary>
    /// تنفيذ المطابقة الثلاثية لفاتورة محددة
    /// يُستدعى عند ربط فاتورة المورد بـ GRN
    /// </summary>
    public async Task<MatchResult> ExecuteMatchAsync(
        Guid invoiceId,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        // 1. جلب بيانات الفاتورة
        var invoice = await _context.PurchaseInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CompanyId == companyId, cancellationToken)
            ?? throw new InvalidOperationException($"Invoice {invoiceId} not found for company {companyId}");

        // 2. جلب بيانات أمر الشراء
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.Lines)
            .FirstOrDefaultAsync(po => po.Id == invoice.PurchaseOrderId, cancellationToken)
            ?? throw new InvalidOperationException($"Purchase Order {invoice.PurchaseOrderId} not found");

        // 3. جلب بيانات سند الاستلام (GRN)
        var grn = await _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .Where(g => g.PurchaseOrderId == invoice.PurchaseOrderId && g.CompanyId == companyId && g.Status == GRNStatus.Approved)
            .OrderByDescending(g => g.ReceiptDate)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"No approved GRN found for Purchase Order {invoice.PurchaseOrderId}");

        // 4. جلب قواعد التسامح
        var toleranceRules = await _context.MatchToleranceRules
            .Where(r => r.CompanyId == companyId && r.IsActive)
            .ToListAsync(cancellationToken);

        // 5. تنفيذ المطابقة سطراً بسطر
        var lineResults = new List<LineMatchResult>();
        var overallStatus = InvoiceMatchStatus.Matched;

        foreach (var invoiceLine in invoice.Lines)
        {
            var poLine = purchaseOrder.Lines.FirstOrDefault(l => l.Id == invoiceLine.PurchaseOrderLineId)
                ?? throw new InvalidOperationException($"PO Line {invoiceLine.PurchaseOrderLineId} not found in PO {purchaseOrder.Id}");

            var grnLine = grn.Lines.FirstOrDefault(l => l.PurchaseOrderLineId == invoiceLine.PurchaseOrderLineId)
                ?? throw new InvalidOperationException($"No GRN line found for PO Line {invoiceLine.PurchaseOrderLineId}. GRN must cover all invoice lines.");

            var tolerance = GetApplicableTolerance(toleranceRules, invoiceLine.ItemId, invoice.SupplierId);

            var lineResult = MatchLine(invoiceLine, poLine, grnLine, tolerance);
            lineResults.Add(lineResult);

            if (lineResult.Status == LineMatchResultStatus.HardVariance)
                overallStatus = InvoiceMatchStatus.Unmatched;
            else if (lineResult.Status == LineMatchResultStatus.SoftVariance && overallStatus == InvoiceMatchStatus.Matched)
                overallStatus = InvoiceMatchStatus.Exception;
        }

        // 6. تحديث حالة الفاتورة
        invoice.MatchStatus = overallStatus;

        // 7. إذا تمت المطابقة — نقل من GRNI إلى AP Subledger + تسجيل Variance إن وجد
        if (overallStatus == InvoiceMatchStatus.Matched || overallStatus == InvoiceMatchStatus.Exception)
        {
            await _periodService.ValidateOpenAsync(invoice.InvoiceDate, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new MatchResult
        {
            InvoiceId = invoiceId,
            OverallStatus = overallStatus,
            LineResults = lineResults,
            MatchedAt = DateTime.UtcNow
        };
    }

    private LineMatchResult MatchLine(
        PurchaseInvoiceLine invoiceLine,
        PurchaseOrderLine poLine,
        GoodsReceiptNoteLine grnLine,
        MatchToleranceRule? tolerance)
    {
        var priceTolerancePct = tolerance?.PriceTolerancePercent ?? 0m;
        var qtyTolerancePct = tolerance?.QuantityTolerancePercent ?? 0m;

        // فحص الكمية: Invoice.Qty vs GRN.Qty
        var qtyVariancePct = grnLine.ReceivedQuantity == 0
            ? throw new InvalidOperationException($"GRN line for item {invoiceLine.ItemId} has zero received quantity")
            : Math.Abs((invoiceLine.Quantity - grnLine.ReceivedQuantity) / grnLine.ReceivedQuantity * 100);

        // فحص السعر: Invoice.UnitPrice vs PO.UnitCost
        var priceVariancePct = poLine.UnitCost == 0
            ? throw new InvalidOperationException($"PO line for item {invoiceLine.ItemId} has zero unit price")
            : Math.Abs((invoiceLine.UnitPrice - poLine.UnitCost) / poLine.UnitCost * 100);

        var priceVarianceAmount = (invoiceLine.UnitPrice - poLine.UnitCost) * invoiceLine.Quantity;

        LineMatchResultStatus status;
        if (qtyVariancePct > qtyTolerancePct || priceVariancePct > priceTolerancePct)
        {
            // Variance يتجاوز حد التسامح → Hard Variance → يمنع الدفع
            status = LineMatchResultStatus.HardVariance;
        }
        else if (qtyVariancePct > 0 || priceVariancePct > 0)
        {
            // Variance ضمن حد التسامح → Soft Variance → يسمح بالدفع مع تسجيل فرق
            status = LineMatchResultStatus.SoftVariance;
        }
        else
        {
            status = LineMatchResultStatus.Matched;
        }

        return new LineMatchResult
        {
            InvoiceLineId = invoiceLine.Id,
            ItemId = invoiceLine.ItemId,
            POQuantity = poLine.OrderedQuantity,
            GRNQuantity = grnLine.ReceivedQuantity,
            InvoiceQuantity = invoiceLine.Quantity,
            POUnitPrice = poLine.UnitPrice,
            InvoiceUnitPrice = invoiceLine.UnitPrice,
            PriceVariancePercent = priceVariancePct,
            QuantityVariancePercent = qtyVariancePct,
            PriceVarianceAmount = priceVarianceAmount,
            Status = status
        };
    }

    /// <summary>
    /// عند نجاح المطابقة: نقل من GRNI Accrual إلى AP Subledger
    /// Dr: AP Accrued Liability / Cr: AP Control Account
    /// وتسجيل Price Variance إذا وجد
    /// </summary>
    private async Task PostAPRecognitionAsync(
        PurchaseInvoice invoice,
        GoodsReceiptNote grn,
        List<LineMatchResult> lineResults,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var apAccruedLiabilityAccount = await _accountMappingService
            .GetAccountCodeAsync(AccountMappingKey.AccountsPayable, companyId, cancellationToken);

        var apControlAccount = await _accountMappingService
            .GetAccountCodeAsync(AccountMappingKey.AccountsPayable, companyId, cancellationToken);

        var priceVarianceAccount = await _accountMappingService
            .GetAccountCodeAsync(AccountMappingKey.PriceVariance, companyId, cancellationToken);

        var totalInvoiceAmount = invoice.Lines.Sum(l => l.UnitPrice * l.Quantity);
        var totalVarianceAmount = lineResults.Sum(r => r.PriceVarianceAmount);

        var journalLines = new List<JournalLineRequest>
        {
            // Dr: AP Accrued Liability (إغلاق الاستحقاق من GRN)
            new JournalLineRequest(apAccruedLiabilityAccount, totalInvoiceAmount - totalVarianceAmount, 0, "AP Accrual Reversal"),
            // Cr: AP Control Account (الالتزام الفعلي للمورد)
            new JournalLineRequest(apControlAccount, 0, totalInvoiceAmount, "AP Control Liability")
        };

        // Variance Posting — إذا وجد فرق سعر
        if (Math.Abs(totalVarianceAmount) > 0.001m)
        {
            if (totalVarianceAmount > 0)
            {
                // فاتورة أعلى من أمر الشراء → خسارة فرق سعر
                journalLines.Add(new JournalLineRequest(priceVarianceAccount, totalVarianceAmount, 0, "Price Variance Loss"));
            }
            else
            {
                // فاتورة أقل من أمر الشراء → مكسب فرق سعر
                journalLines.Add(new JournalLineRequest(priceVarianceAccount, 0, Math.Abs(totalVarianceAmount), "Price Variance Gain"));
            }
        }

        var period = await _context.AccountingPeriods.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.StartDate <= invoice.InvoiceDate && p.EndDate >= invoice.InvoiceDate, cancellationToken);
        var periodId = period?.Id ?? Guid.Empty;

        await _accountingPostingService.PostAsync(new PostJournalRequest(
            companyId,
            periodId,
            $"AP Recognition upon 3-Way Match — Invoice {invoice.InvoiceNumber}",
            $"AP-MATCH-{invoice.InvoiceNumber}",
            invoice.InvoiceDate,
            journalLines
        ), cancellationToken);
    }

    private static MatchToleranceRule? GetApplicableTolerance(
        List<MatchToleranceRule> rules,
        Guid itemId,
        Guid supplierId)
    {
        // أولوية: Item-Supplier specific → Item only → Supplier only → Default
        return rules.FirstOrDefault(r => r.ItemId == itemId && r.SupplierId == supplierId)
            ?? rules.FirstOrDefault(r => r.ItemId == itemId && r.SupplierId == null)
            ?? rules.FirstOrDefault(r => r.ItemId == null && r.SupplierId == supplierId)
            ?? rules.FirstOrDefault(r => r.ItemId == null && r.SupplierId == null);
    }
}

// ─── Result Models ───────────────────────────────────────────────

public class MatchResult
{
    public Guid InvoiceId { get; init; }
    public InvoiceMatchStatus OverallStatus { get; init; }
    public List<LineMatchResult> LineResults { get; init; } = new();
    public DateTime MatchedAt { get; init; }
}

public class LineMatchResult
{
    public Guid InvoiceLineId { get; init; }
    public Guid ItemId { get; init; }
    public decimal POQuantity { get; init; }
    public decimal GRNQuantity { get; init; }
    public decimal InvoiceQuantity { get; init; }
    public decimal POUnitPrice { get; init; }
    public decimal InvoiceUnitPrice { get; init; }
    public decimal PriceVariancePercent { get; init; }
    public decimal QuantityVariancePercent { get; init; }
    public decimal PriceVarianceAmount { get; init; }
    public LineMatchResultStatus Status { get; init; }
}

public enum LineMatchResultStatus
{
    Matched = 0,
    SoftVariance = 1,   // ضمن حد التسامح — مسموح بالدفع
    HardVariance = 2    // يتجاوز حد التسامح — محظور الدفع
}
