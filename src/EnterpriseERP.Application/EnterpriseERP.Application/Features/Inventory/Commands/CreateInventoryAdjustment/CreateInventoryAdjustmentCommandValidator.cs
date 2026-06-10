using FluentValidation;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryAdjustment;

public class CreateInventoryAdjustmentCommandValidator : AbstractValidator<CreateInventoryAdjustmentCommand>
{
    public CreateInventoryAdjustmentCommandValidator()
    {
        RuleFor(v => v.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(v => v.ItemId)
            .NotEmpty().WithMessage("Item is required.");

        RuleFor(v => v.Quantity)
            .GreaterThan(0).WithMessage("Adjustment quantity must be greater than zero.");

        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Invalid adjustment type.");

        RuleFor(v => v.Reason)
            .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.");
    }
}
