using EnterpriseERP.Application.Features.Customers.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQuery : IRequest<IEnumerable<CustomerDto>>
{
}
