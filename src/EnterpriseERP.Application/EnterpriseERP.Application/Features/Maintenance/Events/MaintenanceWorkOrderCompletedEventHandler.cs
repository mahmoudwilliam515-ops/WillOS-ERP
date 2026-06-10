using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.FixedAssets;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Events.Maintenance;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Maintenance.Events;

public class MaintenanceWorkOrderCompletedEventHandler : INotificationHandler<MaintenanceWorkOrderCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public MaintenanceWorkOrderCompletedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MaintenanceWorkOrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        var asset = await _unitOfWork.Repository<FixedAsset>().GetByIdAsync(notification.AssetId);
        
        if (asset?.WorkCenterId != null)
        {
            var workCenter = await _unitOfWork.Repository<WorkCenter>().GetByIdAsync(asset.WorkCenterId.Value);
            if (workCenter != null)
            {
                workCenter.Status = WorkCenterStatus.Available;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
