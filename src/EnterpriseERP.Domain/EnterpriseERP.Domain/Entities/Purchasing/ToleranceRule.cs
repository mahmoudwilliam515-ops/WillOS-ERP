using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Purchasing;

public enum ToleranceType
{
    Quantity = 0,
    Price = 1,
    Tax = 2
}

public class ToleranceRule : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public ToleranceType Type { get; set; }
    public decimal LowerPercentage { get; set; }
    public decimal UpperPercentage { get; set; }
    public decimal LowerAbsolute { get; set; }
    public decimal UpperAbsolute { get; set; }
    public bool IsActive { get; set; } = true;
}
