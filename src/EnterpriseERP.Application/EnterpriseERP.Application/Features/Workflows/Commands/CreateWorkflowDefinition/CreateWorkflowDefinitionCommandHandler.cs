using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Commands.CreateWorkflowDefinition;

public class CreateWorkflowDefinitionCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateWorkflowDefinitionCommand, WorkflowDefinitionDto>
{
    public async Task<WorkflowDefinitionDto> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            DocumentType = request.DocumentType,
            MinAmount = request.MinAmount,
            MaxAmount = request.MaxAmount,
            RequiredApprovals = request.RequiredApprovals,
            ApproverRole = request.ApproverRole.Trim(),
            IsActive = true
        };

        await unitOfWork.Repository<WorkflowDefinition>().AddAsync(definition);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkflowDefinitionDto
        {
            Id = definition.Id,
            Name = definition.Name,
            DocumentType = definition.DocumentType,
            MinAmount = definition.MinAmount,
            MaxAmount = definition.MaxAmount,
            RequiredApprovals = definition.RequiredApprovals,
            ApproverRole = definition.ApproverRole,
            IsActive = definition.IsActive
        };
    }
}
