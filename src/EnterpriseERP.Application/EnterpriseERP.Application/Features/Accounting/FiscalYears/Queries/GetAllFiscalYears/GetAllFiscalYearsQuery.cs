using EnterpriseERP.Application.Features.Accounting.FiscalYears.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.FiscalYears.Queries.GetAllFiscalYears;

public class GetAllFiscalYearsQuery : IRequest<List<FiscalYearDto>>
{
}
