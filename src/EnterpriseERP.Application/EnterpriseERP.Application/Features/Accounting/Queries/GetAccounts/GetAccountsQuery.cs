using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.Accounting.Queries.GetAccounts;

public record GetAccountsQuery : IRequest<Result<List<AccountDto>>>
{
    public bool? IsActive { get; init; }
    public int? Type { get; init; }
    public Guid? ParentId { get; init; }
}

public record AccountDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool IsLeaf { get; init; }
    public Guid? ParentId { get; init; }
}
