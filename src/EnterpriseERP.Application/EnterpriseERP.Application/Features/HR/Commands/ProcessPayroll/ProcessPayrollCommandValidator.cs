using FluentValidation;

namespace EnterpriseERP.Application.Features.HR.Commands.ProcessPayroll;

public class ProcessPayrollCommandValidator : AbstractValidator<ProcessPayrollCommand>
{
    public ProcessPayrollCommandValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .WithMessage("Payroll year must be between 2000 and 2100.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Payroll month must be between 1 and 12.");
    }
}
