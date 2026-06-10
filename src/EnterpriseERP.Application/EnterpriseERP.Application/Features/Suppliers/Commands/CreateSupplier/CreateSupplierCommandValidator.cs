using FluentValidation;

namespace EnterpriseERP.Application.Features.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(300).WithMessage("Supplier name must not exceed 300 characters.");

        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Supplier code is required.")
            .MaximumLength(50).WithMessage("Supplier code must not exceed 50 characters.");

        RuleFor(v => v.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be zero or greater.");

        RuleFor(v => v.OpeningBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Opening balance must be zero or greater.");
    }
}
