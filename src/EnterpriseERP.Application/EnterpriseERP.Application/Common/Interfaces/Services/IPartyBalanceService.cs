using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IPartyBalanceService
{
    /// <summary>
    /// Recalculates and persists the snapshot balance for a given party (Customer/Supplier).
    /// Call this after any financial transaction is posted or reversed.
    /// </summary>
    Task RecalculateAsync(Guid partyId, string partyType, CancellationToken cancellationToken = default);
}
