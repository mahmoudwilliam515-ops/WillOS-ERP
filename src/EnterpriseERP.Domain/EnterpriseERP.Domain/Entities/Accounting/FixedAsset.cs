using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum DepreciationMethod
{
    StraightLine = 0,
    DecliningBalance = 1,
    DoubleDecliningBalance = 2
}

public class AssetCategory : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal DefaultDepreciationRate { get; set; }
    public DepreciationMethod DefaultMethod { get; set; }
    public int UsefulLifeMonths { get; set; }
}

public class FixedAsset : AuditableEntity, IAggregateRoot
{
    public string AssetNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public AssetCategory Category { get; set; } = null!;

    public DateTime AcquisitionDate { get; set; }
    public decimal PurchaseValue { get; set; }
    public decimal SalvageValue { get; set; } // القيمة التخريدية
    
    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue => PurchaseValue - AccumulatedDepreciation;

    public DepreciationMethod DepreciationMethod { get; set; }
    public decimal DepreciationRate { get; set; }
    public int UsefulLifeMonths { get; set; }
    public DateTime? LastDepreciationDate { get; set; }

    public bool IsActive { get; set; } = true;
    public List<AssetDepreciationLog> DepreciationLogs { get; set; } = new();
}

public class AssetDepreciationLog : BaseEntity
{
    public Guid FixedAssetId { get; set; }
    public DateTime PostingDate { get; set; }
    public decimal Amount { get; set; }
    public decimal NewAccumulatedDepreciation { get; set; }
    public Guid JournalEntryId { get; set; }
}
