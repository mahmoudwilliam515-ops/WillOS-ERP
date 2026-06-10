using MediatR;

namespace EnterpriseERP.Application.Features.FixedAssets.Commands.ProcessDepreciation;

public record ProcessDepreciationCommand : IRequest<Guid>
{
    public int Year { get; set; }
    public int Month { get; set; }
}
