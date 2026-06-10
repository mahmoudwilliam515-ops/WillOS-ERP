using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.StartProductionStage;

public record StartProductionStageCommand(Guid ProductionOrderId, Guid StageId) : IRequest<Result>;

public class StartProductionStageCommandHandler : IRequestHandler<StartProductionStageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartProductionStageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(StartProductionStageCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Repository<ProductionOrder>().Query()
            .Include(o => o.Stages)
            .FirstOrDefaultAsync(o => o.Id == request.ProductionOrderId, cancellationToken);

        if (order == null)
            return Result.Failure(new Error("ProductionOrder.NotFound", "Production order not found."));

        try
        {
            order.StartStage(request.StageId, "System");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ProductionOrder.Error", ex.Message));
        }
    }
}
