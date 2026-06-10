using FluentValidation;

namespace EnterpriseERP.Application.Features.Workflows.Commands.SubmitApprovalRequest;

public class SubmitApprovalRequestCommandValidator : AbstractValidator<SubmitApprovalRequestCommand>
{
    public SubmitApprovalRequestCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.DocumentNumber).MaximumLength(80);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequestedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
