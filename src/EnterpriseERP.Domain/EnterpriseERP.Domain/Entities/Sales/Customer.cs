using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Sales;

public class Customer : AuditableEntity, ISoftDelete
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; } = 0;
    public decimal OpeningBalance { get; set; } = 0;
    public decimal Balance { get; set; } = 0;
    public Guid? SellerId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
