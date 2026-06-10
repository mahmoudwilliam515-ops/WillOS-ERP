using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Branches.DTOs;
using EnterpriseERP.Domain.Entities.Settings;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace EnterpriseERP.Application.Features.Branches.Queries.GetAllBranches;

public class GetAllBranchesQueryHandler : IRequestHandler<GetAllBranchesQuery, IEnumerable<BranchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "Branches_List";

    public GetAllBranchesQueryHandler(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<IEnumerable<BranchDto>> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out IEnumerable<BranchDto>? cachedBranches) && cachedBranches != null)
        {
            return cachedBranches;
        }

        var branches = await _unitOfWork.Repository<Branch>().GetAllAsync();
        
        var result = branches.Select(b => new BranchDto
        {
            Id = b.Id,
            Name = b.Name,
            Address = b.Address,
            Phone = b.Phone,
            IsActive = b.IsActive
        }).ToList();

        _cache.Set(CacheKey, result, TimeSpan.FromMinutes(30));

        return result;
    }
}
