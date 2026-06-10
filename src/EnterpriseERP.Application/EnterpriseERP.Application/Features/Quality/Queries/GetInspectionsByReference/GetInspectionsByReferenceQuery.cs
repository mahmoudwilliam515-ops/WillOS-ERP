using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Quality.Queries.GetInspectionsByReference;

public record GetInspectionsByReferenceQuery(Guid ReferenceId, InspectionType Type) : IRequest<Result<IEnumerable<QualityInspection>>>;

public class GetInspectionsByReferenceQueryHandler : IRequestHandler<GetInspectionsByReferenceQuery, Result<IEnumerable<QualityInspection>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInspectionsByReferenceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<QualityInspection>>> Handle(GetInspectionsByReferenceQuery request, CancellationToken cancellationToken)
    {
        var inspections = await _unitOfWork.Repository<QualityInspection>().Query()
            .Include(i => i.Results)
            .Include(i => i.Checklist)
            .Where(i => i.ReferenceId == request.ReferenceId && i.Type == request.Type)
            .ToListAsync(cancellationToken);

        return Result.Success((IEnumerable<QualityInspection>)inspections);
    }
}
