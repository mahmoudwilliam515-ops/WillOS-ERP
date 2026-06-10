using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Quality.Commands.RequestInspection;

public record RequestInspectionCommand : IRequest<Result<Guid>>
{
    public Guid ChecklistId { get; init; }
    public InspectionType Type { get; init; }
    public Guid ReferenceId { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}
