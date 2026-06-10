using FluentValidation;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.RunConsolidation;

public class RunConsolidationCommandValidator : AbstractValidator<RunConsolidationCommand>
{
    public RunConsolidationCommandValidator()
    {
        RuleFor(x => x.GroupCompanyId).NotEmpty();
        RuleFor(x => x.PeriodStart).NotEmpty();
        RuleFor(x => x.PeriodEnd)
            .GreaterThanOrEqualTo(x => x.PeriodStart)
            .WithMessage("Period end must be on or after period start.");
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
