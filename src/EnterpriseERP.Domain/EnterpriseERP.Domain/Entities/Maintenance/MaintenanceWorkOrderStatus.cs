namespace EnterpriseERP.Domain.Entities.Maintenance;

public enum MaintenanceWorkOrderStatus
{
    Open,
    InProgress,
    WaitingForParts,
    Completed,
    Cancelled
}
