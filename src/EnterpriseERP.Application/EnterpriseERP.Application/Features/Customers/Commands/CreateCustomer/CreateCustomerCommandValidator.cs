using FluentValidation;

namespace EnterpriseERP.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(300).WithMessage("Customer name must not exceed 300 characters.");

        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Customer code is required.")
            .MaximumLength(50).WithMessage("Customer code must not exceed 50 characters.");

        RuleFor(v => v.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be zero or greater.");

        RuleFor(v => v.OpeningBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Opening balance must be zero or greater.");
    }
}
