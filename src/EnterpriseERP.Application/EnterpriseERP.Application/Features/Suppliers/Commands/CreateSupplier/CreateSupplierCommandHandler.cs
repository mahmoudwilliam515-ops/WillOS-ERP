using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Suppliers.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;

namespace EnterpriseERP.Application.Features.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = new Supplier
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Phone = request.Phone,
            Address = request.Address,
            TaxNumber = request.TaxNumber,
            CreditLimit = request.CreditLimit,
            OpeningBalance = request.OpeningBalance,
            Balance = request.OpeningBalance, // Initial balance = opening balance
            Notes = request.Notes,
            IsActive = true
        };

        await _unitOfWork.Repository<Supplier>().AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SupplierDto
        {
            Id = supplier.Id,
            Code = supplier.Code,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Address = supplier.Address,
            TaxNumber = supplier.TaxNumber,
            CreditLimit = supplier.CreditLimit,
            OpeningBalance = supplier.OpeningBalance,
            Balance = supplier.Balance,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive
        };
    }
}
