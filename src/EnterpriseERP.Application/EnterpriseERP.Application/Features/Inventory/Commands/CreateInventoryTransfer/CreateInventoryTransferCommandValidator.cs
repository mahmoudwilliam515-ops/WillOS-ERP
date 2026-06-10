using FluentValidation;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryTransfer;

public class CreateInventoryTransferCommandValidator : AbstractValidator<CreateInventoryTransferCommand>
{
    public CreateInventoryTransferCommandValidator()
    {
        RuleFor(v => v.FromWarehouseId)
            .NotEmpty().WithMessage("Source warehouse is required.");

        RuleFor(v => v.ToWarehouseId)
            .NotEmpty().WithMessage("Destination warehouse is required.")
            .NotEqual(v => v.FromWarehouseId).WithMessage("Source and destination warehouses must be different.");

        RuleFor(v => v.TransferDate)
            .NotEmpty().WithMessage("Transfer date is required.");

        RuleFor(v => v.Remarks)
            .MaximumLength(500).WithMessage("Remarks must not exceed 500 characters.");
    }
}
