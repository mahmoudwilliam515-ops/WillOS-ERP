using EnterpriseERP.SharedKernel.DomainEvents;

namespace EnterpriseERP.Application.Features.FixedAssets.Events;

/// <summary>يُطلق عند تسجيل أصل ثابت جديد</summary>
public class FixedAssetCreatedEvent : BaseDomainEvent
{
    public Guid AssetId { get; }
    public string AssetName { get; }
    public decimal PurchaseValue { get; }

    public FixedAssetCreatedEvent(Guid assetId, string assetName, decimal purchaseValue)
    {
        AssetId = assetId;
        AssetName = assetName;
        PurchaseValue = purchaseValue;
    }
}

/// <summary>يُطلق بعد تشغيل الاستهلاك على الأصول الثابتة</summary>
public class DepreciationProcessedEvent : BaseDomainEvent
{
    public int Year { get; }
    public int Month { get; }
    public decimal TotalDepreciationAmount { get; }

    public DepreciationProcessedEvent(int year, int month, decimal totalDepreciationAmount)
    {
        Year = year;
        Month = month;
        TotalDepreciationAmount = totalDepreciationAmount;
    }
}
