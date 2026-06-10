using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Accounting.Periods.Commands.ClosePeriod;

public record ClosePeriodCommand(Guid PeriodId) : IRequest<bool>;

public class ClosePeriodCommandHandler : IRequestHandler<ClosePeriodCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ClosePeriodCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ClosePeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _unitOfWork.Repository<AccountingPeriod>().GetByIdAsync(request.PeriodId);
        if (period == null) throw new AccountingDomainException("Accounting period not found.");

        if (period.Status == AccountingPeriodStatus.Closed)
            throw new AccountingDomainException("Period is already closed.");

        // 1. Validate Checklist (RULE-IFRS08: Must complete checklist before closing)
        if (!period.IsInventoryReconciled) throw new AccountingDomainException("Inventory must be reconciled before closing.");
        if (!period.IsARReconciled) throw new AccountingDomainException("Accounts Receivable must be reconciled before closing.");
        if (!period.IsAPReconciled) throw new AccountingDomainException("Accounts Payable must be reconciled before closing.");
        if (!period.IsBankReconciled) throw new AccountingDomainException("Bank accounts must be reconciled before closing.");
        if (!period.IsFixedAssetsDepreciated) throw new AccountingDomainException("Depreciation must be posted before closing.");
        if (!period.IsPayrollPosted) throw new AccountingDomainException("Payroll must be posted before closing.");

        // 2. Final check for unposted journal entries in this period
        var unpostedCount = await _unitOfWork.Repository<JournalEntry>().Query()
            .CountAsync(e => e.EntryDate >= period.StartDate && e.EntryDate <= period.EndDate && e.Status != JournalEntryStatus.Posted, cancellationToken);

        if (unpostedCount > 0)
            throw new AccountingDomainException($"There are {unpostedCount} unposted journal entries in this period. Please post or delete them first.");

        // 3. Close the period
        period.Status = AccountingPeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        period.ClosedBy = _currentUserService.UserId ?? "System";

        _unitOfWork.Repository<AccountingPeriod>().Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
