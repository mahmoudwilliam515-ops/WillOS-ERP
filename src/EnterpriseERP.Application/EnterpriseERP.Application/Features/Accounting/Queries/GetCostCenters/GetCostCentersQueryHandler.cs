using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.Queries.GetCostCenters;

public class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, Result<List<CostCenterDto>>>
{
    private readonly IGenericRepository<CostCenter> _costCenterRepo;

    public GetCostCentersQueryHandler(IGenericRepository<CostCenter> costCenterRepo)
    {
        _costCenterRepo = costCenterRepo;
    }

    public async Task<Result<List<CostCenterDto>>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
    {
        var costCenters = await _costCenterRepo.GetAllAsync();

        var query = costCenters.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(cc => cc.IsActive == request.IsActive.Value);

        var result = query.Select(cc => new CostCenterDto
        {
            Id          = cc.Id,
            Code        = cc.Code,
            Name        = cc.Name,
            Description = cc.Description,
            IsActive    = cc.IsActive,
            ParentId    = cc.ParentId
        }).ToList();

        return Result.Success(result);
    }
}
