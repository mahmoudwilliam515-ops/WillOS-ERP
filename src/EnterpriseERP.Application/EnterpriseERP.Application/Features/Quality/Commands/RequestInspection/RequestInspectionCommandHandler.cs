using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Quality.Commands.RequestInspection;

public class RequestInspectionCommandHandler : IRequestHandler<RequestInspectionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestInspectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RequestInspectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var inspection = new QualityInspection
            {
                Id = Guid.NewGuid(),
                InspectionNumber = $"QC-REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                ChecklistId = request.ChecklistId,
                Type = request.Type,
                ReferenceId = request.ReferenceId,
                ReferenceNumber = request.ReferenceNumber,
                Notes = request.Notes,
                Status = InspectionStatus.Pending,
                InspectionDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<QualityInspection>().AddAsync(inspection);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(inspection.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("Quality.RequestError", ex.Message));
        }
    }
}
