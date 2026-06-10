using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Quality.Commands.PerformInspection;

public class PerformInspectionCommandHandler : IRequestHandler<PerformInspectionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public PerformInspectionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(PerformInspectionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var checklist = await _unitOfWork.Repository<QualityChecklist>().Query()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == request.ChecklistId, cancellationToken);

            if (checklist == null)
                return Result.Failure<Guid>(new Error("Quality.ChecklistNotFound", "Quality checklist not found."));

            var inspection = new QualityInspection
            {
                Id = Guid.NewGuid(),
                InspectionNumber = $"QC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                ChecklistId = request.ChecklistId,
                Type = request.Type,
                ReferenceId = request.ReferenceId,
                ReferenceNumber = request.ReferenceNumber,
                InspectorName = request.InspectorName,
                InspectionDate = request.InspectionDate,
                Notes = request.Notes,
                Status = request.Results.All(r => r.IsPassed) ? InspectionStatus.Passed : InspectionStatus.Failed
            };

            foreach (var resultDto in request.Results)
            {
                var checklistItem = checklist.Items.FirstOrDefault(i => i.Id == resultDto.ChecklistItemId);
                inspection.Results.Add(new InspectionResultItem
                {
                    Id = Guid.NewGuid(),
                    ChecklistItemId = resultDto.ChecklistItemId,
                    Requirement = checklistItem?.Requirement ?? "N/A",
                    IsPassed = resultDto.IsPassed,
                    Finding = resultDto.Finding
                });
            }

            await _unitOfWork.Repository<QualityInspection>().AddAsync(inspection);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(inspection.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("Quality.InspectionError", ex.Message));
        }
    }
}
