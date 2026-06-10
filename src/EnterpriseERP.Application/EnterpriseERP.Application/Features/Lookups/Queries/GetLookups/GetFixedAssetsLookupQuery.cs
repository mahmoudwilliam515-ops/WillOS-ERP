using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Lookups.DTOs;
using EnterpriseERP.Domain.Entities.FixedAssets;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Lookups.Queries.GetLookups;

public record GetFixedAssetsLookupQuery(string? SearchTerm) : IRequest<List<LookupDto>>;

public class GetFixedAssetsLookupQueryHandler : IRequestHandler<GetFixedAssetsLookupQuery, List<LookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFixedAssetsLookupQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LookupDto>> Handle(GetFixedAssetsLookupQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<FixedAsset>().Query();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a => a.NameEn.Contains(request.SearchTerm) || a.NameAr.Contains(request.SearchTerm) || a.Code.Contains(request.SearchTerm));
        }

        return await query
            .OrderBy(a => a.NameEn)
            .Select(a => new LookupDto { Id = a.Id, Name = $"{a.Code} - {a.NameEn}" })
            .ToListAsync(cancellationToken);
    }
}
