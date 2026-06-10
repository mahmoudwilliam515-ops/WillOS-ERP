using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.FiscalYears.Commands.CloseAccountingPeriod;

public class CloseAccountingPeriodCommandHandler : IRequestHandler<CloseAccountingPeriodCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public CloseAccountingPeriodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CloseAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _unitOfWork.Repository<AccountingPeriod>().GetByIdAsync(request.PeriodId);
        if (period == null)
            throw new Exception("Period not found");

        period.Status = EnterpriseERP.Domain.Entities.Accounting.AccountingPeriodStatus.Closed;

        _unitOfWork.Repository<AccountingPeriod>().Update(period);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
