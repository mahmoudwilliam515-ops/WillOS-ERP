using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class ProductionSchedulerService : IProductionSchedulerService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductionSchedulerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ScheduleProductionOrderAsync(ProductionOrder order, CancellationToken cancellationToken = default)
    {
        var stages = order.Stages.OrderBy(s => s.Sequence).ToList();
        DateTime currentPointer = order.PlannedStartDate;

        foreach (var stage in stages)
        {
            if (stage.WorkCenterId == null)
            {
                // If no work center, just use estimated hours sequentially
                stage.StartDate = currentPointer;
                stage.EndDate = currentPointer.AddHours((double)stage.EstimatedHours);
                currentPointer = stage.EndDate.Value;
                continue;
            }

            // Find the earliest available slot for this work center starting from currentPointer
            var schedule = await FindEarliestSlotAsync(stage.WorkCenterId.Value, currentPointer, stage.EstimatedHours, cancellationToken);
            
            stage.StartDate = schedule.Start;
            stage.EndDate = schedule.End;
            
            // Next stage starts after this one finishes
            currentPointer = stage.EndDate.Value;
        }

        // Update order planned end date
        order.PlannedEndDate = stages.LastOrDefault()?.EndDate ?? order.PlannedStartDate;
    }

    private async Task<(DateTime Start, DateTime End)> FindEarliestSlotAsync(Guid workCenterId, DateTime preferredStart, decimal requiredHours, CancellationToken cancellationToken)
    {
        DateTime checkDate = preferredStart.Date;
        decimal hoursRemaining = requiredHours;
        DateTime? slotStart = null;
        DateTime? slotEnd = null;

        // Simple algorithm: check day by day until we find enough capacity
        // In a real system, we'd check hour-by-hour gaps
        
        while (hoursRemaining > 0)
        {
            var dailyCapacity = await GetDailyCapacityAsync(workCenterId, checkDate, cancellationToken);
            
            if (dailyCapacity.AvailableHours > 0)
            {
                if (slotStart == null)
                {
                    // Start at the later of preferredStart or the beginning of the work day
                    var dayStart = checkDate.Add(dailyCapacity.StartTime);
                    slotStart = preferredStart > dayStart ? preferredStart : dayStart;
                }

                if (dailyCapacity.AvailableHours >= hoursRemaining)
                {
                    // We found enough in this day
                    slotEnd = slotStart.Value.AddHours((double)hoursRemaining);
                    hoursRemaining = 0;
                }
                else
                {
                    // Use all available hours and move to next day
                    hoursRemaining -= dailyCapacity.AvailableHours;
                    checkDate = checkDate.AddDays(1);
                    // Reset slotStart for next day if needed (logic simplified)
                }
            }
            else
            {
                checkDate = checkDate.AddDays(1);
            }

            // Safety break to prevent infinite loop
            if (checkDate > preferredStart.AddMonths(6)) break;
        }

        return (slotStart ?? preferredStart, slotEnd ?? preferredStart.AddHours((double)requiredHours));
    }

    private async Task<(decimal AvailableHours, TimeSpan StartTime)> GetDailyCapacityAsync(Guid workCenterId, DateTime date, CancellationToken cancellationToken)
    {
        // 1. Check Exceptions
        var exception = await _unitOfWork.Repository<WorkCenterException>().Query()
            .FirstOrDefaultAsync(e => e.WorkCenterId == workCenterId && e.Date.Date == date.Date, cancellationToken);

        if (exception != null)
        {
            if (!exception.IsWorkingDay) return (0, TimeSpan.Zero);
            var hours = (decimal)(exception.EndTime?.Subtract(exception.StartTime ?? TimeSpan.Zero).TotalHours ?? 0);
            return (hours, exception.StartTime ?? TimeSpan.Zero);
        }

        // 2. Check Regular Calendar
        var dayOfWeek = date.DayOfWeek;
        var calendar = await _unitOfWork.Repository<WorkCenterCalendar>().Query()
            .FirstOrDefaultAsync(c => c.WorkCenterId == workCenterId && c.DayOfWeek == dayOfWeek, cancellationToken);

        if (calendar == null || !calendar.IsWorkingDay) return (0, TimeSpan.Zero);

        // 3. Check existing load (simplified: subtract hours from total)
        var occupiedHours = await _unitOfWork.Repository<ProductionOrderStage>().Query()
            .Where(s => s.WorkCenterId == workCenterId && s.StartDate.HasValue && s.StartDate.Value.Date == date.Date)
            .SumAsync(s => s.EstimatedHours, cancellationToken);

        var available = Math.Max(0, calendar.AvailableHours - occupiedHours);
        return (available, calendar.StartTime);
    }

    public async Task<List<WorkCenterAvailabilityDto>> GetWorkCenterAvailabilityAsync(Guid workCenterId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var result = new List<WorkCenterAvailabilityDto>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var capacity = await GetDailyCapacityAsync(workCenterId, date, cancellationToken);
            
            // Re-calculate occupied to be accurate
            var occupied = await _unitOfWork.Repository<ProductionOrderStage>().Query()
                .Where(s => s.WorkCenterId == workCenterId && s.StartDate.HasValue && s.StartDate.Value.Date == date.Date)
                .SumAsync(s => s.EstimatedHours, cancellationToken);

            result.Add(new WorkCenterAvailabilityDto
            {
                Date = date,
                TotalHours = capacity.AvailableHours + occupied,
                OccupiedHours = occupied
            });
        }
        return result;
    }
}
