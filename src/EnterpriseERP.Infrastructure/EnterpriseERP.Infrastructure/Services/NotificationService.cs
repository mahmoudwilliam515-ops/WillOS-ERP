using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Settings;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SendSystemAlertAsync(string title, string message, string severity, string? link = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = title,
            Message = message,
            Severity = MapSeverity(severity),
            IsRead = false,
            Link = link,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
        
        Console.WriteLine($"[SYSTEM ALERT - {severity}] {title}: {message}");
    }

    public async Task SendUserNotificationAsync(string userId, string title, string message, string? link = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = title,
            Message = message,
            Severity = NotificationSeverity.Info,
            IsRead = false,
            UserId = userId,
            Link = link,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"[USER NOTIFICATION - {userId}] {title}: {message}");
    }

    private NotificationSeverity MapSeverity(string severity)
    {
        return severity.ToLower() switch
        {
            "error" => NotificationSeverity.Error,
            "warning" => NotificationSeverity.Warning,
            "success" => NotificationSeverity.Success,
            _ => NotificationSeverity.Info
        };
    }
}
