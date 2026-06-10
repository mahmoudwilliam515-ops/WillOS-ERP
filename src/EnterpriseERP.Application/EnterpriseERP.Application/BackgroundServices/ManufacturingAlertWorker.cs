using System;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.Application.BackgroundServices;

public class ManufacturingAlertWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ManufacturingAlertWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(4); // Run every 4 hours

    public ManufacturingAlertWorker(IServiceProvider serviceProvider, ILogger<ManufacturingAlertWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Manufacturing Alert Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                    _logger.LogInformation("Checking manufacturing alerts at: {time}", DateTimeOffset.Now);
                    await alertService.CheckManufacturingAlertsAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking manufacturing alerts.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Manufacturing Alert Worker is stopping.");
    }
}
