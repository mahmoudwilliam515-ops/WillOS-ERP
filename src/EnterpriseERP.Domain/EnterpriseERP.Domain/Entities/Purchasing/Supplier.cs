using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Purchasing;

public class Supplier : AuditableEntity, ISoftDelete
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public string SwiftCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; } = 0;
    public decimal OpeningBalance { get; set; } = 0;
    public decimal Balance { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
