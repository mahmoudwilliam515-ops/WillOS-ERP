using FluentValidation;

namespace EnterpriseERP.Application.Features.Workflows.Commands.ActOnApprovalRequest;

public class ActOnApprovalRequestCommandValidator : AbstractValidator<ActOnApprovalRequestCommand>
{
    public ActOnApprovalRequestCommandValidator()
    {
        RuleFor(x => x.ApprovalRequestId).NotEmpty();
        RuleFor(x => x.ActorUserId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
