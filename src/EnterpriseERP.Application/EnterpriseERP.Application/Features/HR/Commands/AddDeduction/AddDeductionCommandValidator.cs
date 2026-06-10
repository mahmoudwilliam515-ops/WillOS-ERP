using FluentValidation;

namespace EnterpriseERP.Application.Features.HR.Commands.AddDeduction;

public class AddDeductionCommandValidator : AbstractValidator<AddDeductionCommand>
{
    public AddDeductionCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty()
            .WithMessage("Employee ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Deduction amount must be greater than zero.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Year must be between 2000 and 2100.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters.");
    }
}
