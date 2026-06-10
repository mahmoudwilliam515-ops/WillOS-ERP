using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Items.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;

namespace EnterpriseERP.Application.Features.Items.Commands.CreateItem;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Barcode = request.Barcode,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            BuyPrice = request.BuyPrice,
            MinStock = request.MinStock,
            MaxStock = request.MaxStock,
            ReorderPoint = request.ReorderPoint,
            TrackSerial = request.TrackSerial,
            HasExpiry = request.HasExpiry,
            IsService = request.IsService,
            IsActive = true
        };

        await _unitOfWork.Repository<Item>().AddAsync(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ItemDto
        {
            Id = item.Id,
            Code = item.Code,
            Barcode = item.Barcode,
            NameAr = item.NameAr,
            NameEn = item.NameEn,
            BuyPrice = item.BuyPrice,
            MinStock = item.MinStock,
            MaxStock = item.MaxStock,
            ReorderPoint = item.ReorderPoint,
            TrackSerial = item.TrackSerial,
            HasExpiry = item.HasExpiry,
            IsService = item.IsService,
            IsActive = item.IsActive
        };
    }
}
