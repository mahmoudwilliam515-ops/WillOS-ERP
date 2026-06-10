using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CreateCostCenter;

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, Result<Guid>>
{
    private readonly IGenericRepository<CostCenter> _costCenterRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCostCenterCommandHandler(
        IGenericRepository<CostCenter> costCenterRepo,
        IUnitOfWork unitOfWork)
    {
        _costCenterRepo = costCenterRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var costCenter = new CostCenter
        {
            Code        = request.Code,
            Name        = request.Name,
            Description = request.Description,
            ParentId    = request.ParentId,
            IsActive    = true
        };

        await _costCenterRepo.AddAsync(costCenter);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(costCenter.Id);
    }
}
