using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Companies.DTOs;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var duplicate = (await unitOfWork.Repository<Company>().FindAsync(c => c.Code == request.Code)).Any();
        if (duplicate)
        {
            throw new AccountingDomainException($"Company code '{request.Code}' already exists.");
        }

        if (request.ParentCompanyId.HasValue)
        {
            var parent = await unitOfWork.Repository<Company>().GetByIdAsync(request.ParentCompanyId.Value);
            if (parent == null)
            {
                throw new AccountingDomainException("Parent company not found.");
            }
        }

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            CountryCode = request.CountryCode.Trim().ToUpperInvariant(),
            FunctionalCurrencyCode = request.FunctionalCurrencyCode.Trim().ToUpperInvariant(),
            TaxRegistrationNumber = request.TaxRegistrationNumber?.Trim() ?? string.Empty,
            LegalEntityType = request.LegalEntityType,
            ParentCompanyId = request.ParentCompanyId,
            IsConsolidationEntity = request.IsConsolidationEntity,
            IsActive = true
        };

        await unitOfWork.Repository<Company>().AddAsync(company);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(company);
    }

    private static CompanyDto ToDto(Company company) => new()
    {
        Id = company.Id,
        Code = company.Code,
        Name = company.Name,
        CountryCode = company.CountryCode,
        FunctionalCurrencyCode = company.FunctionalCurrencyCode,
        TaxRegistrationNumber = company.TaxRegistrationNumber,
        LegalEntityType = company.LegalEntityType,
        ParentCompanyId = company.ParentCompanyId,
        IsConsolidationEntity = company.IsConsolidationEntity,
        IsActive = company.IsActive
    };
}
