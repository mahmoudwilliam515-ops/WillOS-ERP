using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.FiscalYears.Commands.CreateFiscalYear;

public class CreateFiscalYearCommandHandler : IRequestHandler<CreateFiscalYearCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateFiscalYearCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateFiscalYearCommand request, CancellationToken cancellationToken)
    {
        var fiscalYear = new FiscalYear
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Year = request.StartDate.Year,
            Status = EnterpriseERP.Domain.Entities.Accounting.FiscalYearStatus.Open
        };

        // Create 12 Accounting Periods automatically
        for (int i = 0; i < 12; i++)
        {
            var periodStart = request.StartDate.AddMonths(i);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);

            fiscalYear.Periods.Add(new AccountingPeriod
            {
                Id = Guid.NewGuid(),
                FiscalYearId = fiscalYear.Id,
                PeriodName = $"{request.Name}-P{i + 1:D2}",
                StartDate = periodStart,
                EndDate = periodEnd,
                Status = EnterpriseERP.Domain.Entities.Accounting.AccountingPeriodStatus.Open
            });
        }

        await _unitOfWork.Repository<FiscalYear>().AddAsync(fiscalYear);
        await _unitOfWork.SaveChangesAsync();

        return fiscalYear.Id;
    }
}
