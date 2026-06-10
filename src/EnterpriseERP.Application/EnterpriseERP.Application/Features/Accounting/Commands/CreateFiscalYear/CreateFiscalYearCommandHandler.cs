using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CreateFiscalYear;

public class CreateFiscalYearCommandHandler : IRequestHandler<CreateFiscalYearCommand, Result<Guid>>
{
    private readonly IGenericRepository<FiscalYear> _fiscalYearRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFiscalYearCommandHandler(
        IGenericRepository<FiscalYear> fiscalYearRepo,
        IUnitOfWork unitOfWork)
    {
        _fiscalYearRepo = fiscalYearRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateFiscalYearCommand request, CancellationToken cancellationToken)
    {
        if (request.EndDate <= request.StartDate)
            throw new AccountingDomainException("End date must be after start date.");

        var existing = await _fiscalYearRepo.GetAllAsync();
        bool overlaps = existing.Any(fy =>
            fy.Status != FiscalYearStatus.Closed &&
            fy.StartDate < request.EndDate &&
            fy.EndDate > request.StartDate);

        if (overlaps)
            throw new AccountingDomainException("A fiscal year with overlapping dates already exists.");

        // Auto-generate 12 monthly accounting periods
        var periods = new System.Collections.Generic.List<AccountingPeriod>();
        var periodStart = request.StartDate;
        int periodIndex = 1;
        while (periodStart < request.EndDate)
        {
            var periodEnd = periodStart.AddMonths(1);
            if (periodEnd > request.EndDate) periodEnd = request.EndDate;
            periods.Add(new AccountingPeriod
            {
                Id        = Guid.NewGuid(),
                PeriodName = $"Period {periodIndex:D2} - {periodStart:MMM yyyy}",
                StartDate = periodStart,
                EndDate   = periodEnd,
                Status    = AccountingPeriodStatus.Open
            });
            periodStart = periodEnd;
            periodIndex++;
        }

        var fiscalYear = new FiscalYear
        {
            Id        = Guid.NewGuid(),
            Name      = request.Name,
            Year      = request.StartDate.Year,
            StartDate = request.StartDate,
            EndDate   = request.EndDate,
            Status    = FiscalYearStatus.Open,
            Periods   = periods
        };

        await _fiscalYearRepo.AddAsync(fiscalYear);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(fiscalYear.Id);
    }
}
