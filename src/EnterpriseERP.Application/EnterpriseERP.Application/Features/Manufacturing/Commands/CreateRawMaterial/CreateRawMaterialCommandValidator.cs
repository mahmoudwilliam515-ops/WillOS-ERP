using FluentValidation;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CreateRawMaterial;

public class CreateRawMaterialCommandValidator : AbstractValidator<CreateRawMaterialCommand>
{
    public CreateRawMaterialCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Unit).NotEmpty().WithMessage("Unit is required.");
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0).WithMessage("Min stock cannot be negative.");
        RuleFor(x => x.CostPerUnit).GreaterThanOrEqualTo(0).WithMessage("Cost per unit cannot be negative.");
    }
}
