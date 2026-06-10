using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Commands.CreateWorkflowDefinition;

public class CreateWorkflowDefinitionCommand : IRequest<WorkflowDefinitionDto>
{
    public string Name { get; set; } = string.Empty;
    public WorkflowDocumentType DocumentType { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int RequiredApprovals { get; set; } = 1;
    public string ApproverRole { get; set; } = string.Empty;
}
