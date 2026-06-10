using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Lookups.DTOs;
using EnterpriseERP.Domain.Entities.Maintenance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Lookups.Queries.GetLookups;

public record GetTechniciansLookupQuery(string? SearchTerm) : IRequest<List<LookupDto>>;

public class GetTechniciansLookupQueryHandler : IRequestHandler<GetTechniciansLookupQuery, List<LookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTechniciansLookupQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LookupDto>> Handle(GetTechniciansLookupQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<MaintenanceTechnician>().Query();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(t => t.Employee.FirstName.Contains(request.SearchTerm) || t.Employee.LastName.Contains(request.SearchTerm));
        }

        return await query
            .OrderBy(t => t.Employee.FirstName)
            .Select(t => new LookupDto { Id = t.Id, Name = t.Employee.FirstName + " " + t.Employee.LastName })
            .ToListAsync(cancellationToken);
    }
}
