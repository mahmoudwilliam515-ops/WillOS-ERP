using EnterpriseERP.API.Hubs;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseERP.API.BackgroundServices;

public class LowStockMonitorService : BackgroundService
{
    private readonly ILogger<LowStockMonitorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<InventoryHub> _hubContext;

    private static readonly TransactionType[] InTypes = 
    {
        TransactionType.PurchaseIn,
        TransactionType.AdjustmentIn,
        TransactionType.TransferIn,
        TransactionType.ReturnFromCustomer
    };

    private static readonly TransactionType[] OutTypes = 
    {
        TransactionType.SalesOut,
        TransactionType.AdjustmentOut,
        TransactionType.TransferOut,
        TransactionType.ReturnToSupplier
    };

    public LowStockMonitorService(
        ILogger<LowStockMonitorService> logger,
        IServiceProvider serviceProvider,
        IHubContext<InventoryHub> hubContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Low Stock Monitor Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckLowStockAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Low Stock Monitor.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CheckLowStockAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var itemRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Item>>();
        var txRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<InventoryTransaction>>();

        // Only items with a ReorderPoint set
        var allItems = await itemRepo.GetAllAsync();
        var trackedItems = allItems
            .Where(i => i.ReorderPoint > 0 && i.IsActive && !i.IsDeleted)
            .ToList();

        if (!trackedItems.Any()) return;

        var allTxs = await txRepo.GetAllAsync();
        var lowStockAlerts = new List<object>();

        foreach (var item in trackedItems)
        {
            var itemTxs = allTxs.Where(t => t.ItemId == item.Id).ToList();

            // Positive quantity = IN, Negative = OUT (as per domain comments)
            // But we use the Type enum to determine direction
            var totalIn  = itemTxs.Where(t => InTypes.Contains(t.Type)).Sum(t => t.Quantity);
            var totalOut = itemTxs.Where(t => OutTypes.Contains(t.Type)).Sum(t => Math.Abs(t.Quantity));
            var balance  = totalIn - totalOut;

            if (balance <= item.ReorderPoint)
            {
                lowStockAlerts.Add(new
                {
                    ItemId       = item.Id,
                    ItemCode     = item.Code,
                    ItemName     = item.NameAr,
                    CurrentStock = balance,
                    ReorderPoint = item.ReorderPoint,
                    MinStock     = item.MinStock
                });
            }
        }

        if (lowStockAlerts.Any())
        {
            _logger.LogWarning("Broadcasting {Count} low-stock alerts.", lowStockAlerts.Count);
            await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", lowStockAlerts, cancellationToken);
        }
    }
}
