using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Suppliers.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;

namespace EnterpriseERP.Application.Features.Suppliers.Queries.GetAllSuppliers;

public class GetAllSuppliersQueryHandler : IRequestHandler<GetAllSuppliersQuery, IEnumerable<SupplierDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSuppliersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SupplierDto>> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await _unitOfWork.Repository<Supplier>().GetAllAsync();

        return suppliers.Select(s => new SupplierDto
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name,
            Phone = s.Phone,
            Address = s.Address,
            TaxNumber = s.TaxNumber,
            CreditLimit = s.CreditLimit,
            OpeningBalance = s.OpeningBalance,
            Balance = s.Balance,
            Notes = s.Notes,
            IsActive = s.IsActive
        });
    }
}
