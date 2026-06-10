using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Customers.DTOs;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;

namespace EnterpriseERP.Application.Features.Customers.Commands.CreateCustomer;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Phone = request.Phone,
            Mobile = request.Mobile,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            CreditLimit = request.CreditLimit,
            OpeningBalance = request.OpeningBalance,
            Balance = request.OpeningBalance, // Initial balance = opening balance
            Notes = request.Notes,
            IsActive = true
        };

        await _unitOfWork.Repository<Customer>().AddAsync(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            Phone = customer.Phone,
            Mobile = customer.Mobile,
            Address = customer.Address,
            TaxNumber = customer.TaxNumber,
            CreditLimit = customer.CreditLimit,
            OpeningBalance = customer.OpeningBalance,
            Balance = customer.Balance,
            Notes = customer.Notes,
            IsActive = customer.IsActive
        };
    }
}
