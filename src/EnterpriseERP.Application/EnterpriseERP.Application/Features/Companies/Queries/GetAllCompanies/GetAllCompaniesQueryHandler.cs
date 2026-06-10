using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Companies.DTOs;
using EnterpriseERP.Domain.Entities.Settings;
using MediatR;

namespace EnterpriseERP.Application.Features.Companies.Queries.GetAllCompanies;

public class GetAllCompaniesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllCompaniesQuery, IEnumerable<CompanyDto>>
{
    public async Task<IEnumerable<CompanyDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await unitOfWork.Repository<Company>().GetAllAsync();

        return companies
            .Where(c => !request.ActiveOnly || c.IsActive)
            .OrderBy(c => c.Code)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                CountryCode = c.CountryCode,
                FunctionalCurrencyCode = c.FunctionalCurrencyCode,
                TaxRegistrationNumber = c.TaxRegistrationNumber,
                LegalEntityType = c.LegalEntityType,
                ParentCompanyId = c.ParentCompanyId,
                IsConsolidationEntity = c.IsConsolidationEntity,
                IsActive = c.IsActive
            })
            .ToList();
    }
}
