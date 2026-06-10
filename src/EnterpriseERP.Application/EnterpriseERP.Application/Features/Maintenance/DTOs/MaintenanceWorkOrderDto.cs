using System;

namespace EnterpriseERP.Application.Features.Maintenance.DTOs;

public record MaintenanceWorkOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid AssetId { get; init; }
    public string? AssetName { get; init; }
    public Guid? AssignedTechnicianId { get; init; }
    public string? TechnicianName { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? ResolutionNotes { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? CompletionDate { get; init; }
    public decimal TotalPartsCost { get; init; }
    public decimal TotalLaborCost { get; init; }
    public decimal TotalCost { get; init; }
    public DateTime CreatedAt { get; init; }
}
