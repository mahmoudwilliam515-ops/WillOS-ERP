using EnterpriseERP.Application.Features.Companies.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Companies.Queries.GetAllCompanies;

public class GetAllCompaniesQuery : IRequest<IEnumerable<CompanyDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
