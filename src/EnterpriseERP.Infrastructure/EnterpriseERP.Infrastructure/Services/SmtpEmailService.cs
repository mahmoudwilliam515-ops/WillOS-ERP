using System.Net;
using System.Net.Mail;
using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var host = _configuration["EmailSettings:Host"];
            var portString = _configuration["EmailSettings:Port"];
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var from = _configuration["EmailSettings:FromAddress"] ?? "noreply@enterpriseerp.com";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Email settings are not fully configured. Email was not sent.");
                return;
            }

            int port = int.TryParse(portString, out var p) ? p : 587;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent to {ToAddress} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToAddress}", to);
            // We usually don't want to throw and crash the calling process just because an email failed.
        }
    }

    public async Task SendCriticalAlertAsync(string subject, string message)
    {
        var adminEmail = _configuration["EmailSettings:AdminEmail"] ?? "admin@enterpriseerp.com";
        await SendEmailAsync(adminEmail, $"[CRITICAL ALERT] {subject}", message);
    }
}
