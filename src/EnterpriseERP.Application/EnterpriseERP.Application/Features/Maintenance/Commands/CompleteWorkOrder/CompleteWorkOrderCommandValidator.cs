using EnterpriseERP.Application.Features.Maintenance.Commands.CompleteWorkOrder;
using FluentValidation;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.CompleteWorkOrder;

public class CompleteWorkOrderCommandValidator : AbstractValidator<CompleteWorkOrderCommand>
{
    public CompleteWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work Order Id is required.");

        RuleFor(x => x.ResolutionNotes)
            .NotEmpty().WithMessage("Resolution notes are required.");

        RuleFor(x => x.TotalPartsCost)
            .GreaterThanOrEqualTo(0).WithMessage("Parts cost cannot be negative.");

        RuleFor(x => x.TotalLaborCost)
            .GreaterThanOrEqualTo(0).WithMessage("Labor cost cannot be negative.");
    }
}
