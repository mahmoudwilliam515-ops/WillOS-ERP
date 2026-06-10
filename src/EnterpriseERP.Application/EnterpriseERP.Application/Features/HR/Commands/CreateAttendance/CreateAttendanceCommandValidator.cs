using FluentValidation;

namespace EnterpriseERP.Application.Features.HR.Commands.CreateAttendance;

public class CreateAttendanceCommandValidator : AbstractValidator<CreateAttendanceCommand>
{
    public CreateAttendanceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee is required.");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.Status).InclusiveBetween(0, 3).WithMessage("Invalid attendance status.");
    }
}
