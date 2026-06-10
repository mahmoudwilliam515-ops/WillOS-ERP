using MediatR;

namespace EnterpriseERP.Application.Features.HR.Commands.ProcessPayroll;

public record ProcessPayrollCommand : IRequest<Guid>
{
    public int Year { get; set; }
    public int Month { get; set; }
}
