using EnterpriseERP.Application.Features.Manufacturing.Commands.StartProductionOrder;
using FluentValidation;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.StartProductionOrder;

public class StartProductionOrderCommandValidator : AbstractValidator<StartProductionOrderCommand>
{
    public StartProductionOrderCommandValidator()
    {
        RuleFor(x => x.ProductionOrderId)
            .NotEmpty().WithMessage("Production Order Id is required.");
    }
}
