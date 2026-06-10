using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.FixedAssets;
using MediatR;

namespace EnterpriseERP.Application.Features.FixedAssets.Commands.CreateFixedAsset;

public class CreateFixedAssetCommandHandler : IRequestHandler<CreateFixedAssetCommand, Guid>
{
    private readonly IGenericRepository<FixedAsset> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFixedAssetCommandHandler(IGenericRepository<FixedAsset> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateFixedAssetCommand request, CancellationToken cancellationToken)
    {
        var fixedAsset = new FixedAsset
        {
            Code = request.Code,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            PurchaseDate = request.PurchaseDate,
            PurchaseCost = request.PurchaseCost,
            SalvageValue = request.SalvageValue,
            UsefulLifeYears = request.UsefulLifeYears,
            DepreciationMethod = request.DepreciationMethod,
            BranchId = request.BranchId,
            AccumulatedDepreciation = 0,
            NetBookValue = request.PurchaseCost,
            IsActive = true
        };

        await _repository.AddAsync(fixedAsset);

        // Add Domain Event
        fixedAsset.AddDomainEvent(new EnterpriseERP.Application.Features.FixedAssets.Events.FixedAssetCreatedEvent(
            fixedAsset.Id, fixedAsset.NameEn, fixedAsset.PurchaseCost));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return fixedAsset.Id;
    }
}
