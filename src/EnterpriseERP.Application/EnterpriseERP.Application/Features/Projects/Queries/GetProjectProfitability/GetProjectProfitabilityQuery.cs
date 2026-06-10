using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Projects.Queries.GetProjectProfitability;

public record GetProjectProfitabilityQuery(Guid ProjectId) : IRequest<ProjectProfitabilityDto>;

public class ProjectProfitabilityDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal ActualLaborCost { get; set; }
    public decimal ActualMaterialCost { get; set; }
    public decimal ActualExpenseCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal CommitmentAmount { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal ProfitabilityPercentage { get; set; }
    public List<TaskCostDto> TaskCosts { get; set; } = new();
}

public class TaskCostDto
{
    public string TaskName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Actual { get; set; }
    public decimal Commitment { get; set; }
    public decimal Variance => Budget - Actual - Commitment;
}

public class GetProjectProfitabilityQueryHandler : IRequestHandler<GetProjectProfitabilityQuery, ProjectProfitabilityDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectProfitabilityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectProfitabilityDto> Handle(GetProjectProfitabilityQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>().Query()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null) throw new KeyNotFoundException("Project not found.");

        var totalActual = project.ActualLaborCost + project.ActualMaterialCost + project.ActualExpenseCost;
        var totalCommitted = project.CommitmentAmount;

        return new ProjectProfitabilityDto
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectCode = project.Code,
            BudgetAmount = project.BudgetAmount,
            ActualLaborCost = project.ActualLaborCost,
            ActualMaterialCost = project.ActualMaterialCost,
            ActualExpenseCost = project.ActualExpenseCost,
            TotalActualCost = totalActual,
            CommitmentAmount = totalCommitted,
            RemainingBudget = project.BudgetAmount - totalActual - totalCommitted,
            ProfitabilityPercentage = project.BudgetAmount > 0 ? ((project.BudgetAmount - totalActual) / project.BudgetAmount) * 100 : 0,
            TaskCosts = project.Tasks.Select(t => new TaskCostDto
            {
                TaskName = t.Name,
                Budget = t.BudgetAmount,
                Actual = t.ActualLaborCost + t.ActualMaterialCost + t.ActualExpenseCost,
                Commitment = t.CommitmentAmount
            }).ToList()
        };
    }
}
