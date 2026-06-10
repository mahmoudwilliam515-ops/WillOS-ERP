using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Domain.Entities.Manufacturing;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IProductionSchedulerService
{
    /// <summary>
    /// Schedules a production order based on work center availability and existing load.
    /// </summary>
    Task ScheduleProductionOrderAsync(ProductionOrder order, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the availability for a work center over a date range.
    /// </summary>
    Task<List<WorkCenterAvailabilityDto>> GetWorkCenterAvailabilityAsync(Guid workCenterId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

public class WorkCenterAvailabilityDto
{
    public DateTime Date { get; set; }
    public decimal TotalHours { get; set; }
    public decimal OccupiedHours { get; set; }
    public decimal AvailableHours => Math.Max(0, TotalHours - OccupiedHours);
}
