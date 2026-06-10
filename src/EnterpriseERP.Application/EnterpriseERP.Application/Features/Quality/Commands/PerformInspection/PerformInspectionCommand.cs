using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using EnterpriseERP.Domain.Entities.Quality;

namespace EnterpriseERP.Application.Features.Quality.Commands.PerformInspection;

public class PerformInspectionCommand : IRequest<Result<Guid>>
{
    public Guid ChecklistId { get; set; }
    public InspectionType Type { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string InspectorName { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
    public List<InspectionResultDto> Results { get; set; } = new();
}

public class InspectionResultDto
{
    public Guid ChecklistItemId { get; set; }
    public bool IsPassed { get; set; }
    public string Finding { get; set; } = string.Empty;
}
