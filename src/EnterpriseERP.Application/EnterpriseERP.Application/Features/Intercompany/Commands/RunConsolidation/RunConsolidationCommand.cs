using EnterpriseERP.Application.Features.Intercompany.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.RunConsolidation;

public class RunConsolidationCommand : IRequest<ConsolidationRunDto>
{
    public Guid GroupCompanyId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string? Notes { get; set; }
}
