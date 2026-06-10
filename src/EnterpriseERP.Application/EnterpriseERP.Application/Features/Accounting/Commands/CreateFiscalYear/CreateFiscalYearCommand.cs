using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CreateFiscalYear;

public record CreateFiscalYearCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
