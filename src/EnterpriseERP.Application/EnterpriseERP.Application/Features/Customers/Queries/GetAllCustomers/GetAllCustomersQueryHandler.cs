using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Customers.DTOs;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;

namespace EnterpriseERP.Application.Features.Customers.Queries.GetAllCustomers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Repository<Customer>().GetAllAsync();

        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Phone = c.Phone,
            Mobile = c.Mobile,
            Address = c.Address,
            TaxNumber = c.TaxNumber,
            CreditLimit = c.CreditLimit,
            OpeningBalance = c.OpeningBalance,
            Balance = c.Balance,
            Notes = c.Notes,
            IsActive = c.IsActive
        });
    }
}
