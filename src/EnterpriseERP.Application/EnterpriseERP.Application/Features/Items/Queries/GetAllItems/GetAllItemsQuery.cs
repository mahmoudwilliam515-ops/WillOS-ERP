using EnterpriseERP.Application.Features.Items.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Items.Queries.GetAllItems;

public class GetAllItemsQuery : IRequest<IEnumerable<ItemDto>>
{
}
