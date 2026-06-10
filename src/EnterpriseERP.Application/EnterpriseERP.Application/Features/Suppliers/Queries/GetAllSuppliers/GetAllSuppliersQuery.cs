using EnterpriseERP.Application.Features.Suppliers.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Suppliers.Queries.GetAllSuppliers;

public class GetAllSuppliersQuery : IRequest<IEnumerable<SupplierDto>>
{
}
