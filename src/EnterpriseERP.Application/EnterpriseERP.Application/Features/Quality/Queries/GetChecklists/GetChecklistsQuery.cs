using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Quality.Queries.GetChecklists;

public record GetChecklistsQuery : IRequest<Result<IEnumerable<QualityChecklist>>>;

public class GetChecklistsQueryHandler : IRequestHandler<GetChecklistsQuery, Result<IEnumerable<QualityChecklist>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChecklistsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<QualityChecklist>>> Handle(GetChecklistsQuery request, CancellationToken cancellationToken)
    {
        var checklists = await _unitOfWork.Repository<QualityChecklist>().Query()
            .Include(c => c.Items)
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        return Result.Success((IEnumerable<QualityChecklist>)checklists);
    }
}
