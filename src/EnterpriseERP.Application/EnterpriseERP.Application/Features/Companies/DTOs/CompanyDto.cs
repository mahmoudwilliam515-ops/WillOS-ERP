using EnterpriseERP.Domain.Entities.Settings;

namespace EnterpriseERP.Application.Features.Companies.DTOs;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string FunctionalCurrencyCode { get; set; } = string.Empty;
    public string TaxRegistrationNumber { get; set; } = string.Empty;
    public LegalEntityType LegalEntityType { get; set; }
    public Guid? ParentCompanyId { get; set; }
    public bool IsConsolidationEntity { get; set; }
    public bool IsActive { get; set; }
}
