using EnterpriseERP.Application.Features.Items.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Items.Commands.CreateItem;

public class CreateItemCommand : IRequest<ItemDto>
{
    public string Code { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
    public int ReorderPoint { get; set; }
    public bool TrackSerial { get; set; }
    public bool HasExpiry { get; set; }
    public bool IsService { get; set; }
}
