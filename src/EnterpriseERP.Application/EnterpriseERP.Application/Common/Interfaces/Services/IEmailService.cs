namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendCriticalAlertAsync(string subject, string message);
}
