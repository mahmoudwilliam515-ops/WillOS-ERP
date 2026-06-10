// ============================================================
// P0-Task-5: إصلاح FIFO — استثناء RemainingQuantity > 0
// الملف: src/Domain/Services/FIFOValuationService.cs
// ============================================================
// المشكلة: الكود الحالي يكمل بصمت حتى لو RemainingQuantity > 0
// الإصلاح: Exception صريح إذا المخزون لا يكفي لتغطية الكمية
// القاعدة: لا Fallback صامت — Exception دائماً
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using EnterpriseERP.SharedKernel.Exceptions;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Exceptions;

namespace EnterpriseERP.Domain.Services;

/// <summary>
/// خدمة تقييم المخزون بطريقة FIFO (First In, First Out)
/// تحسب تكلفة الوحدات المُصرَفة بناءً على أقدم دفعات الاستلام
/// </summary>
public class FIFOValuationService : IFIFOValuationService
{
    /// <summary>
    /// يحسب تكلفة FIFO لكمية مُصرَفة ويُرجع قائمة الـ Layers المستهلكة.
    ///
    /// يرمي FIFOValuationException إذا:
    /// - لا توجد Layers كافية
    /// - مجموع الكميات في الـ Layers أقل من الكمية المطلوبة
    /// </summary>
    /// <param name="fifoLayers">طبقات المخزون مرتبة تصاعدياً بتاريخ الاستلام</param>
    /// <param name="quantityToDeduct">الكمية المراد صرفها</param>
    /// <param name="itemId">معرّف الصنف (للرسالة)</param>
    /// <returns>قائمة الـ Consumptions التي تُجمع تكلفة FIFO</returns>
    public IReadOnlyList<FIFOConsumption> CalculateFIFOCost(
        IEnumerable<InventoryFIFOLayer> fifoLayers,
        decimal quantityToDeduct,
        Guid itemId)
    {
        if (quantityToDeduct <= 0)
            throw new InventoryDomainException(
                $"الكمية المراد صرفها يجب أن تكون موجبة. القيمة: {quantityToDeduct}");

        // رتِّب الـ Layers بتاريخ الاستلام (الأقدم أولاً) — FIFO
        var orderedLayers = fifoLayers
            .Where(l => l.RemainingQuantity > 0)
            .OrderBy(l => l.ReceivedDate)
            .ThenBy(l => l.CreatedAt)
            .ToList();

        if (!orderedLayers.Any())
            throw new FIFOValuationException(
                $"لا توجد طبقات مخزون (FIFO Layers) للصنف {itemId}. " +
                $"لا يمكن حساب تكلفة FIFO بدون layers.");

        var totalAvailable = orderedLayers.Sum(l => l.RemainingQuantity);

        if (totalAvailable < quantityToDeduct)
            throw new FIFOValuationException(
                $"المخزون المتاح ({totalAvailable:N4} وحدة) غير كافٍ لتغطية الكمية " +
                $"المطلوبة ({quantityToDeduct:N4} وحدة) للصنف {itemId}. " +
                $"العجز: {(quantityToDeduct - totalAvailable):N4} وحدة. " +
                $"تحقق من إدخال استلام بضاعة (GRN) قبل إصدار هذه الفاتورة.");

        var consumptions = new List<FIFOConsumption>();
        var remainingToDeduct = quantityToDeduct;

        foreach (var layer in orderedLayers)
        {
            if (remainingToDeduct <= 0)
                break;

            var consumedFromLayer = Math.Min(layer.RemainingQuantity, remainingToDeduct);

            consumptions.Add(new FIFOConsumption(
                LayerId: layer.Id,
                ConsumedQuantity: consumedFromLayer,
                UnitCost: layer.UnitCost,
                TotalCost: consumedFromLayer * layer.UnitCost,
                ReceivedDate: layer.ReceivedDate));

            // تخفيض الـ Layer
            layer.Consume(consumedFromLayer);
            remainingToDeduct -= consumedFromLayer;
        }

        // تحقق نهائي — يجب أن يكون صفراً الآن
        if (remainingToDeduct > 0)
        {
            // هذا لا يحدث أبداً بسبب فحص totalAvailable أعلاه
            // لكن نحتفظ به كـ safety net
            throw new FIFOValuationException(
                $"خطأ داخلي في حساب FIFO: لا تزال هناك كمية غير مُعالَجة " +
                $"({remainingToDeduct:N4}) بعد استهلاك كل الـ Layers للصنف {itemId}. " +
                $"هذا يشير إلى خلل في البيانات — يرجى مراجعة FIFO Layers.");
        }

        return consumptions.AsReadOnly();
    }

    /// <summary>
    /// يحسب متوسط تكلفة FIFO للوحدات الباقية في المخزون
    /// (للعرض في التقارير)
    /// </summary>
    public decimal CalculateAverageUnitCost(
        IEnumerable<InventoryFIFOLayer> fifoLayers,
        Guid itemId)
    {
        var activeLayers = fifoLayers
            .Where(l => l.RemainingQuantity > 0)
            .ToList();

        if (!activeLayers.Any())
            return 0m;

        var totalValue = activeLayers.Sum(l => l.RemainingQuantity * l.UnitCost);
        var totalQuantity = activeLayers.Sum(l => l.RemainingQuantity);

        return totalQuantity > 0 ? totalValue / totalQuantity : 0m;
    }
}

public record FIFOConsumption(
    Guid LayerId,
    decimal ConsumedQuantity,
    decimal UnitCost,
    decimal TotalCost,
    DateTime ReceivedDate);
