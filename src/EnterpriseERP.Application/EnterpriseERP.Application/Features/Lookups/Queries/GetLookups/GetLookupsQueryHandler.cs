using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Lookups.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Settings;
using MediatR;
using System.Linq.Expressions;

namespace EnterpriseERP.Application.Features.Lookups.Queries.GetLookups;

public class GetLookupsQueryHandler : 
    IRequestHandler<GetCustomersLookupQuery, IEnumerable<LookupDto>>,
    IRequestHandler<GetSuppliersLookupQuery, IEnumerable<LookupDto>>,
    IRequestHandler<GetWarehousesLookupQuery, IEnumerable<LookupDto>>,
    IRequestHandler<GetItemsLookupQuery, IEnumerable<ItemLookupDto>>,
    IRequestHandler<GetBranchesLookupQuery, IEnumerable<LookupDto>>
{
    private readonly IUnitOfWork _uow;

    public GetLookupsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<LookupDto>> Handle(GetCustomersLookupQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Customer, bool>> filter = x => string.IsNullOrEmpty(request.SearchTerm) || x.Name.Contains(request.SearchTerm) || x.Code.Contains(request.SearchTerm);
        var entities = await _uow.Repository<Customer>().FindAsync(filter);
        return entities.Take(50).Select(x => new LookupDto { Id = x.Id, Name = x.Name, Code = x.Code });
    }

    public async Task<IEnumerable<LookupDto>> Handle(GetSuppliersLookupQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Supplier, bool>> filter = x => string.IsNullOrEmpty(request.SearchTerm) || x.Name.Contains(request.SearchTerm) || x.Code.Contains(request.SearchTerm);
        var entities = await _uow.Repository<Supplier>().FindAsync(filter);
        return entities.Take(50).Select(x => new LookupDto { Id = x.Id, Name = x.Name, Code = x.Code });
    }

    public async Task<IEnumerable<LookupDto>> Handle(GetWarehousesLookupQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Warehouse, bool>> filter = x => string.IsNullOrEmpty(request.SearchTerm) || x.Name.Contains(request.SearchTerm);
        var entities = await _uow.Repository<Warehouse>().FindAsync(filter);
        return entities.Take(50).Select(x => new LookupDto { Id = x.Id, Name = x.Name, Code = string.Empty });
    }

    public async Task<IEnumerable<ItemLookupDto>> Handle(GetItemsLookupQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Item, bool>> filter = x => string.IsNullOrEmpty(request.SearchTerm) || x.NameAr.Contains(request.SearchTerm) || x.NameEn.Contains(request.SearchTerm) || x.Code.Contains(request.SearchTerm);
        var entities = await _uow.Repository<Item>().FindAsync(filter);
        return entities.Take(50).Select(x => new ItemLookupDto { Id = x.Id, Name = x.NameAr, Code = x.Code, Price = x.BuyPrice }); // Adjust price if needed
    }

    public async Task<IEnumerable<LookupDto>> Handle(GetBranchesLookupQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Branch, bool>> filter = x => string.IsNullOrEmpty(request.SearchTerm) || x.Name.Contains(request.SearchTerm);
        var entities = await _uow.Repository<Branch>().FindAsync(filter);
        return entities.Take(50).Select(x => new LookupDto { Id = x.Id, Name = x.Name, Code = string.Empty });
    }
}
