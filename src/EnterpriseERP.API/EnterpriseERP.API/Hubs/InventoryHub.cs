using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EnterpriseERP.API.Hubs;

[Authorize]
public class InventoryHub : Hub
{
    // Clients will connect to this Hub to receive "ReceiveLowStockAlert" messages
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
