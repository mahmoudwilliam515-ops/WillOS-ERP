using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Services;

public class PurchaseMatchingService : IPurchaseMatchingService
{
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseMatchingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> PerformThreeWayMatchAsync(Guid purchaseInvoiceId)
    {
        var invoice = await _unitOfWork.Repository<PurchaseInvoice>().Query()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == purchaseInvoiceId);

        if (invoice == null) return Result.Failure<bool>(new Error("PurchaseInvoice.NotFound", "Invoice not found."));

        var (success, message, status) = await MatchInvoiceAsync(invoice);

        invoice.Status = status;
        invoice.Notes = AppendSystemNote(invoice.Notes, success ? "3-way matching successful." : "P2P match failed: " + message);

        await _unitOfWork.SaveChangesAsync(default);

        return success ? Result.Success(true) : Result.Failure<bool>(new Error("PurchaseInvoice.MatchFailed", message));
    }

    public async Task<(bool Success, string Message, PurchaseInvoiceStatus ResultStatus)> MatchInvoiceAsync(PurchaseInvoice invoice)
    {
        var messages = new List<string>();
        bool hasDiscrepancy = false;

        // Load Tolerance Rules
        var toleranceRules = (await _unitOfWork.Repository<ToleranceRule>().GetAllAsync()).Where(r => r.IsActive).ToList();
        
        var qtyTolerance = toleranceRules.FirstOrDefault(r => r.Type == ToleranceType.Quantity);
        var priceTolerance = toleranceRules.FirstOrDefault(r => r.Type == ToleranceType.Price);
        var taxTolerance = toleranceRules.FirstOrDefault(r => r.Type == ToleranceType.Tax);

        foreach (var line in invoice.Lines)
        {
            // 1. Check against Purchase Order Line
            if (line.PurchaseOrderLineId.HasValue)
            {
                var poLine = await _unitOfWork.Repository<PurchaseOrderLine>().GetByIdAsync(line.PurchaseOrderLineId.Value);
                if (poLine != null)
                {
                    // Price Check
                    if (Math.Abs(line.UnitCost - poLine.UnitCost) > 0)
                    {
                        var diff = Math.Abs(line.UnitCost - poLine.UnitCost);
                        var allowedDiff = priceTolerance != null 
                            ? Math.Max(poLine.UnitCost * (priceTolerance.UpperPercentage / 100m), priceTolerance.UpperAbsolute)
                            : 0;

                        if (diff > allowedDiff)
                        {
                            hasDiscrepancy = true;
                            messages.Add($"Line {line.ItemId}: Price mismatch. Invoice: {line.UnitCost}, PO: {poLine.UnitCost}. Diff: {diff} exceeds tolerance {allowedDiff}.");
                        }
                    }

                    // Tax Check
                    if (Math.Abs(line.TaxAmount - poLine.TaxAmount) > 0)
                    {
                        var diff = Math.Abs(line.TaxAmount - poLine.TaxAmount);
                        var allowedDiff = taxTolerance != null
                            ? Math.Max(poLine.TaxAmount * (taxTolerance.UpperPercentage / 100m), taxTolerance.UpperAbsolute)
                            : 0;

                        if (diff > allowedDiff)
                        {
                            hasDiscrepancy = true;
                            messages.Add($"Line {line.ItemId}: Tax mismatch. Invoice: {line.TaxAmount}, PO: {poLine.TaxAmount}. Diff: {diff} exceeds tolerance {allowedDiff}.");
                        }
                    }
                }
            }

            // 2. Check against Goods Receipt Line
            if (line.GoodsReceiptLineId.HasValue)
            {
                var grnLine = await _unitOfWork.Repository<GoodsReceiptNoteLine>().GetByIdAsync(line.GoodsReceiptLineId.Value);
                if (grnLine != null)
                {
                    // Quantity Check
                    if (line.Quantity > grnLine.ReceivedQuantity)
                    {
                        var diff = line.Quantity - grnLine.ReceivedQuantity;
                        var allowedDiff = qtyTolerance != null
                            ? Math.Max(grnLine.ReceivedQuantity * (qtyTolerance.UpperPercentage / 100m), qtyTolerance.UpperAbsolute)
                            : 0;

                        if (diff > allowedDiff)
                        {
                            hasDiscrepancy = true;
                            messages.Add($"Line {line.ItemId}: Quantity mismatch. Invoice: {line.Quantity}, GRN: {grnLine.ReceivedQuantity}. Diff: {diff} exceeds tolerance {allowedDiff}.");
                        }
                    }
                }
                else
                {
                    hasDiscrepancy = true;
                    messages.Add($"Line {line.ItemId}: Goods Receipt Line not found for matching.");
                }
            }
            else if (invoice.PurchaseOrderId.HasValue)
            {
                // If PO exists but no GRN line linked, it's a discrepancy if system requires 3-way matching
                hasDiscrepancy = true;
                messages.Add($"Line {line.ItemId}: Missing Goods Receipt Line for 3-way matching.");
            }
        }

        if (hasDiscrepancy)
        {
            return (false, string.Join(" | ", messages), PurchaseInvoiceStatus.OnHold);
        }

        return (true, "3-way matching successful.", PurchaseInvoiceStatus.Matched);
    }

    private static string AppendSystemNote(string currentNotes, string note)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var systemNote = $"[{timestamp} UTC] {note}";

        return string.IsNullOrWhiteSpace(currentNotes)
            ? systemNote
            : $"{currentNotes.Trim()}{Environment.NewLine}{systemNote}";
    }
}

