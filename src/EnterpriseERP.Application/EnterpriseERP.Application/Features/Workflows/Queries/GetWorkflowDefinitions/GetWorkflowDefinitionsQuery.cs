using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Queries.GetWorkflowDefinitions;

public class GetWorkflowDefinitionsQuery : IRequest<IEnumerable<WorkflowDefinitionDto>>
{
    public WorkflowDocumentType? DocumentType { get; set; }
    public bool ActiveOnly { get; set; } = true;
}
