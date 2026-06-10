using EnterpriseERP.Application.Features.Maintenance.Commands.StartWorkOrder;
using FluentValidation;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.StartWorkOrder;

public class StartWorkOrderCommandValidator : AbstractValidator<StartWorkOrderCommand>
{
    public StartWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId)
            .NotEmpty().WithMessage("Work Order Id is required.");
    }
}
