using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CreateRawMaterial;

public class CreateRawMaterialCommand : IRequest<Result<Guid>>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal MinStock { get; set; }
    public decimal CostPerUnit { get; set; }
}
