using FluentValidation;

namespace EnterpriseERP.Application.Features.Items.Commands.CreateItem;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(v => v.NameAr)
            .NotEmpty().WithMessage("Arabic item name is required.")
            .MaximumLength(300).WithMessage("Arabic name must not exceed 300 characters.");

        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Item code is required.")
            .MaximumLength(50).WithMessage("Item code must not exceed 50 characters.");

        RuleFor(v => v.BuyPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Buy price must be zero or greater.");

        RuleFor(v => v.MinStock)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock must be zero or greater.");
    }
}
