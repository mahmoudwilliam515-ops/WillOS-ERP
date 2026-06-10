using FluentValidation;

namespace EnterpriseERP.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Warehouse name is required.")
            .MaximumLength(200).WithMessage("Warehouse name must not exceed 200 characters.");

        RuleFor(v => v.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");
    }
}
