using FluentValidation;

namespace EnterpriseERP.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Branch name is required.")
            .MaximumLength(200).WithMessage("Branch name must not exceed 200 characters.");
    }
}
