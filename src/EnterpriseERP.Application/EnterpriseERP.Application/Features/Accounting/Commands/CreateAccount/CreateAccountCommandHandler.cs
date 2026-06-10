using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CreateAccount;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    private readonly IGenericRepository<Account> _accountRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(
        IGenericRepository<Account> accountRepo,
        IUnitOfWork unitOfWork)
    {
        _accountRepo = accountRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new Account
        {
            Code = request.Code,
            Name = request.Name,
            Type = (AccountType)request.Type,
            ParentId = request.ParentId,
            IsActive = true,
            IsLeaf = true
        };

        await _accountRepo.AddAsync(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(account.Id);
    }
}
