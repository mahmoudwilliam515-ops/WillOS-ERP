using FluentValidation;

namespace EnterpriseERP.Application.Features.Workflows.Commands.CreateWorkflowDefinition;

public class CreateWorkflowDefinitionCommandValidator : AbstractValidator<CreateWorkflowDefinitionCommand>
{
    public CreateWorkflowDefinitionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.MinAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxAmount)
            .GreaterThanOrEqualTo(x => x.MinAmount)
            .When(x => x.MaxAmount.HasValue);
        RuleFor(x => x.RequiredApprovals).GreaterThan(0).LessThanOrEqualTo(10);
        RuleFor(x => x.ApproverRole).MaximumLength(100);
    }
}
