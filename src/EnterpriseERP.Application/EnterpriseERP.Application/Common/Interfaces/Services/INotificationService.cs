using System;
using EnterpriseERP.Application.Common.Interfaces.Services;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Services;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface INotificationService
{
    Task SendSystemAlertAsync(string title, string message, string severity, string? link = null);
    Task SendUserNotificationAsync(string userId, string title, string message, string? link = null);
}
