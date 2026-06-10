using EnterpriseERP.Domain.Entities.Workflow;

namespace EnterpriseERP.Application.Features.Workflows.DTOs;

public class WorkflowDefinitionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkflowDocumentType DocumentType { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int RequiredApprovals { get; set; }
    public string ApproverRole { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
