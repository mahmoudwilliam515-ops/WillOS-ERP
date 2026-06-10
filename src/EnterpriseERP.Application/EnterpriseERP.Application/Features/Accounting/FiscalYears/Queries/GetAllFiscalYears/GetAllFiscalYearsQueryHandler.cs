using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Accounting.FiscalYears.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.FiscalYears.Queries.GetAllFiscalYears;

public class GetAllFiscalYearsQueryHandler : IRequestHandler<GetAllFiscalYearsQuery, List<FiscalYearDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllFiscalYearsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FiscalYearDto>> Handle(GetAllFiscalYearsQuery request, CancellationToken cancellationToken)
    {
        var years = (await _unitOfWork.Repository<FiscalYear>().FindAsync(_ => true))
            .OrderByDescending(y => y.StartDate)
            .ToList();

        var yearIds = years.Select(y => y.Id).ToList();
        var periods = yearIds.Count > 0
            ? (await _unitOfWork.Repository<AccountingPeriod>().FindAsync(p => yearIds.Contains(p.FiscalYearId))).ToList()
            : new List<AccountingPeriod>();

        return years.Select(y => new FiscalYearDto
        {
            Id = y.Id,
            Name = y.Name,
            StartDate = y.StartDate,
            EndDate = y.EndDate,
            IsClosed = y.Status == FiscalYearStatus.Closed,
            Periods = periods
                .Where(p => p.FiscalYearId == y.Id)
                .OrderBy(p => p.StartDate)
                .Select(p => new AccountingPeriodDto
                {
                    Id = p.Id,
                    Name = p.PeriodName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsClosed = p.Status == AccountingPeriodStatus.Closed
                })
                .ToList()
        }).ToList();
    }
}
