using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CreateRawMaterial;

public class CreateRawMaterialCommandHandler : IRequestHandler<CreateRawMaterialCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRawMaterialCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateRawMaterialCommand request, CancellationToken cancellationToken)
    {
        var rawMaterial = new RawMaterial
        {
            Code = request.Code,
            Name = request.Name,
            Unit = request.Unit,
            MinStockLevel = request.MinStock,
            CostPerUnit = request.CostPerUnit,
            StockLevel = 0
        };

        await _unitOfWork.Repository<RawMaterial>().AddAsync(rawMaterial);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(rawMaterial.Id);
    }
}
