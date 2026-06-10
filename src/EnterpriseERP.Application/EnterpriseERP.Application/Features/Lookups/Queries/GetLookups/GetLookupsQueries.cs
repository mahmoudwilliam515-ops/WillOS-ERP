using EnterpriseERP.Application.Features.Lookups.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Lookups.Queries.GetLookups;

public record GetCustomersLookupQuery(string? SearchTerm) : IRequest<IEnumerable<LookupDto>>;
public record GetSuppliersLookupQuery(string? SearchTerm) : IRequest<IEnumerable<LookupDto>>;
public record GetWarehousesLookupQuery(string? SearchTerm) : IRequest<IEnumerable<LookupDto>>;
public record GetItemsLookupQuery(string? SearchTerm) : IRequest<IEnumerable<ItemLookupDto>>;
public record GetBranchesLookupQuery(string? SearchTerm) : IRequest<IEnumerable<LookupDto>>;
