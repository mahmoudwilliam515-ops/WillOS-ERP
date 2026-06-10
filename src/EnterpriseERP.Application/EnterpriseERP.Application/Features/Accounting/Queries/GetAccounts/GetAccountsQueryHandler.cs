using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.Queries.GetAccounts;

public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, Result<List<AccountDto>>>
{
    private readonly IGenericRepository<Account> _accountRepo;

    public GetAccountsQueryHandler(IGenericRepository<Account> accountRepo)
    {
        _accountRepo = accountRepo;
    }

    public async Task<Result<List<AccountDto>>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepo.GetAllAsync();

        var query = accounts.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(a => a.IsActive == request.IsActive.Value);

        if (request.Type.HasValue)
            query = query.Where(a => (int)a.Type == request.Type.Value);

        if (request.ParentId.HasValue)
            query = query.Where(a => a.ParentId == request.ParentId.Value);

        var result = query.Select(a => new AccountDto
        {
            Id       = a.Id,
            Code     = a.Code,
            Name     = a.Name,
            Type     = a.Type.ToString(),
            IsActive = a.IsActive,
            IsLeaf   = a.IsLeaf,
            ParentId = a.ParentId
        }).ToList();

        return Result.Success(result);
    }
}
