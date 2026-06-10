using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CreateAccount;

public record CreateAccountCommand : IRequest<Result<Guid>>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Type { get; init; }
    public Guid? ParentId { get; init; }
}
