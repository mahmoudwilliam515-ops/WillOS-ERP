using EnterpriseERP.Application.Features.Manufacturing.Commands.CompleteProductionOrder;
using FluentValidation;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CompleteProductionOrder;

public class CompleteProductionOrderCommandValidator : AbstractValidator<CompleteProductionOrderCommand>
{
    public CompleteProductionOrderCommandValidator()
    {
        RuleFor(x => x.ProductionOrderId)
            .NotEmpty().WithMessage("Production Order Id is required.");

        RuleFor(x => x.ProducedQuantity)
            .GreaterThan(0).WithMessage("Produced quantity must be greater than zero.");

        RuleFor(x => x.TotalLaborCost)
            .GreaterThanOrEqualTo(0).WithMessage("Labor cost cannot be negative.");

        RuleFor(x => x.TotalMaterialCost)
            .GreaterThanOrEqualTo(0).WithMessage("Material cost cannot be negative.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Target warehouse ID is required.");
    }
}
