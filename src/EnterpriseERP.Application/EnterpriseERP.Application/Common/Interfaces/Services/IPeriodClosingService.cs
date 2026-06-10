using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Application.Common.Interfaces.Services;
using System;
using EnterpriseERP.Application.Common.Interfaces.Services;
using System.Threading;
using EnterpriseERP.Application.Common.Interfaces.Services;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Services;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IPeriodClosingService
{
    Task<bool> IsOpenAsync(DateTime date, CancellationToken cancellationToken = default);
    Task ValidateOpenAsync(DateTime date, CancellationToken cancellationToken = default);
    
    Task<AccountingPeriod> ClosePeriodAsync(Guid periodId, string closedBy, CancellationToken cancellationToken = default);
    Task<bool> ValidateChecklistAsync(Guid periodId, CancellationToken cancellationToken = default);
}
