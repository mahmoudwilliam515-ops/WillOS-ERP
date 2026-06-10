using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Settings;

public enum LegalEntityType
{
    Company = 0,
    BranchOffice = 1,
    JointVenture = 2,
    Holding = 3,
    Subsidiary = 4
}

public class Company : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string FunctionalCurrencyCode { get; set; } = string.Empty;
    public string TaxRegistrationNumber { get; set; } = string.Empty;
    public LegalEntityType LegalEntityType { get; set; } = LegalEntityType.Company;
    public Guid? ParentCompanyId { get; set; }
    public Company? ParentCompany { get; set; }
    public bool IsConsolidationEntity { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Company> Subsidiaries { get; set; } = new List<Company>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
