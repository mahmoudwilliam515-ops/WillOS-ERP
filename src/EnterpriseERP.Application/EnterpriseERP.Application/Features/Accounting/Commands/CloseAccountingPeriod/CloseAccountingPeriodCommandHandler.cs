using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CloseAccountingPeriod;

public class CloseAccountingPeriodCommandHandler : IRequestHandler<CloseAccountingPeriodCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CloseAccountingPeriodCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CloseAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _unitOfWork.Repository<AccountingPeriod>().GetByIdAsync(request.PeriodId);
        
        if (period == null)
            return Result.Failure<Guid>(new Error("Period.NotFound", "Accounting period not found."));
            
        if (period.Status == AccountingPeriodStatus.Closed)
            return Result.Failure<Guid>(new Error("Period.AlreadyClosed", "Accounting period is already closed."));

        period.Status = AccountingPeriodStatus.Closed;
        _unitOfWork.Repository<AccountingPeriod>().Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(period.Id);
    }
}
