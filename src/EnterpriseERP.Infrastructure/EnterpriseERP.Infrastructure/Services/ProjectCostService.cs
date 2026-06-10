using System;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class ProjectCostService : IProjectCostService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectCostService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task UpdateCommitmentAsync(Guid projectTaskId, decimal amount, bool isAddition, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Repository<ProjectTask>().Query()
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == projectTaskId, cancellationToken);

        if (task == null) return;

        if (isAddition)
        {
            task.CommitmentAmount += amount;
            task.Project.CommitmentAmount += amount;
        }
        else
        {
            task.CommitmentAmount -= amount;
            task.Project.CommitmentAmount -= amount;
        }

        _unitOfWork.Repository<ProjectTask>().Update(task);
        _unitOfWork.Repository<Project>().Update(task.Project);
    }

    public async Task UpdateActualCostAsync(Guid projectTaskId, decimal amount, string costType, CancellationToken cancellationToken = default)
    {
        var task = await _unitOfWork.Repository<ProjectTask>().Query()
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == projectTaskId, cancellationToken);

        if (task == null) return;

        switch (costType.ToLower())
        {
            case "labor":
                task.ActualLaborCost += amount;
                task.Project.ActualLaborCost += amount;
                break;
            case "material":
                task.ActualMaterialCost += amount;
                task.Project.ActualMaterialCost += amount;
                break;
            case "expense":
                task.ActualExpenseCost += amount;
                task.Project.ActualExpenseCost += amount;
                break;
        }

        _unitOfWork.Repository<ProjectTask>().Update(task);
        _unitOfWork.Repository<Project>().Update(task.Project);
    }
}
