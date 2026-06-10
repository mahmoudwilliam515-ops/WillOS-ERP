using EnterpriseERP.Application.Features.Companies.DTOs;
using EnterpriseERP.Domain.Entities.Settings;
using MediatR;

namespace EnterpriseERP.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommand : IRequest<CompanyDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public string FunctionalCurrencyCode { get; set; } = string.Empty;
    public string? TaxRegistrationNumber { get; set; }
    public LegalEntityType LegalEntityType { get; set; } = LegalEntityType.Company;
    public Guid? ParentCompanyId { get; set; }
    public bool IsConsolidationEntity { get; set; }
}
