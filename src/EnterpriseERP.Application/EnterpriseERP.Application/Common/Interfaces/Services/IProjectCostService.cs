using System;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Projects;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IProjectCostService
{
    Task UpdateCommitmentAsync(Guid projectTaskId, decimal amount, bool isAddition, CancellationToken cancellationToken = default);
    Task UpdateActualCostAsync(Guid projectTaskId, decimal amount, string costType, CancellationToken cancellationToken = default);
}
