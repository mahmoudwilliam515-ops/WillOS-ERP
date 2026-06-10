using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CompleteProductionStage;

public record CompleteProductionStageCommand(Guid ProductionOrderId, Guid StageId, decimal ActualHours) : IRequest<Result>;

public class CompleteProductionStageCommandHandler : IRequestHandler<CompleteProductionStageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteProductionStageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteProductionStageCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Repository<ProductionOrder>().Query()
            .Include(o => o.Stages)
            .FirstOrDefaultAsync(o => o.Id == request.ProductionOrderId, cancellationToken);

        if (order == null)
            return Result.Failure(new Error("ProductionOrder.NotFound", "Production order not found."));

        try
        {
            order.CompleteStage(request.StageId, request.ActualHours, "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductionOrder.Error", ex.Message));
        }
    }
}
