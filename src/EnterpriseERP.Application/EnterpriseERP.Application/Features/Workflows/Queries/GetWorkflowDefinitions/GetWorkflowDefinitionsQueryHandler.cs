using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Queries.GetWorkflowDefinitions;

public class GetWorkflowDefinitionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetWorkflowDefinitionsQuery, IEnumerable<WorkflowDefinitionDto>>
{
    public async Task<IEnumerable<WorkflowDefinitionDto>> Handle(GetWorkflowDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var definitions = await unitOfWork.Repository<WorkflowDefinition>().GetAllAsync();

        return definitions
            .Where(d => !request.DocumentType.HasValue || d.DocumentType == request.DocumentType.Value)
            .Where(d => !request.ActiveOnly || d.IsActive)
            .OrderBy(d => d.DocumentType)
            .ThenBy(d => d.MinAmount)
            .Select(d => new WorkflowDefinitionDto
            {
                Id = d.Id,
                Name = d.Name,
                DocumentType = d.DocumentType,
                MinAmount = d.MinAmount,
                MaxAmount = d.MaxAmount,
                RequiredApprovals = d.RequiredApprovals,
                ApproverRole = d.ApproverRole,
                IsActive = d.IsActive
            })
            .ToList();
    }
}
