using EnterpriseERP.Domain.Entities.HR;
using MediatR;

namespace EnterpriseERP.Application.Features.HR.Commands.AddDeduction;

public record AddDeductionCommand : IRequest<Guid>
{
    public Guid EmployeeId { get; init; }
    public DeductionType DeductionType { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
}
