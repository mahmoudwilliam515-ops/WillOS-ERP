using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.FixedAssets;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Events.Maintenance;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Maintenance.Events;

public class MaintenanceWorkOrderStartedEventHandler : INotificationHandler<MaintenanceWorkOrderStartedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public MaintenanceWorkOrderStartedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MaintenanceWorkOrderStartedEvent notification, CancellationToken cancellationToken)
    {
        var asset = await _unitOfWork.Repository<FixedAsset>().GetByIdAsync(notification.AssetId);
        
        if (asset?.WorkCenterId != null)
        {
            var workCenter = await _unitOfWork.Repository<WorkCenter>().GetByIdAsync(asset.WorkCenterId.Value);
            if (workCenter != null)
            {
                workCenter.Status = WorkCenterStatus.UnderMaintenance;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
