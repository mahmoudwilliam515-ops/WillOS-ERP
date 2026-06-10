using FluentValidation;

namespace EnterpriseERP.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty().WithMessage("Supplier is required.");
        RuleFor(x => x.BranchId).NotEmpty().WithMessage("Branch is required.");
        RuleFor(x => x.WarehouseId).NotEmpty().WithMessage("Warehouse is required.");
        RuleFor(x => x.ExpectedDeliveryDate).GreaterThanOrEqualTo(x => x.OrderDate).WithMessage("Expected delivery date must be after or equal to order date.");
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one purchase order line is required.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ItemId).NotEmpty().WithMessage("Item is required.");
            line.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            line.RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit cost cannot be negative.");
            line.RuleFor(x => x.TaxPercent).InclusiveBetween(0, 100).WithMessage("Tax percent must be between 0 and 100.");
        });
    }
}
