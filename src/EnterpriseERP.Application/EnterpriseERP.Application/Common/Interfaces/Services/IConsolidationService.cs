using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IConsolidationService
{
    /// <summary>
    /// Performs a financial consolidation for a group company for a specific period.
    /// </summary>
    Task<Guid> RunConsolidationAsync(Guid groupCompanyId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken = default);
}
