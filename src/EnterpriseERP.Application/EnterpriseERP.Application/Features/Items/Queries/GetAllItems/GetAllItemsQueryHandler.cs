using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Items.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;

namespace EnterpriseERP.Application.Features.Items.Queries.GetAllItems;

public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, IEnumerable<ItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllItemsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ItemDto>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Repository<Item>().GetAllAsync();

        return items.Select(i => new ItemDto
        {
            Id = i.Id,
            Code = i.Code,
            Barcode = i.Barcode,
            NameAr = i.NameAr,
            NameEn = i.NameEn,
            BuyPrice = i.BuyPrice,
            MinStock = i.MinStock,
            MaxStock = i.MaxStock,
            ReorderPoint = i.ReorderPoint,
            TrackSerial = i.TrackSerial,
            HasExpiry = i.HasExpiry,
            IsService = i.IsService,
            IsActive = i.IsActive
        });
    }
}
