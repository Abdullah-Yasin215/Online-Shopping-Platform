// Hubs/StockHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace train.Hubs
{
    [Authorize(Policy = "AdminOnly")]
    public class StockHub : Hub
    {
        public const string GroupName = "Admins";

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
            await base.OnDisconnectedAsync(exception);
        }
    }
}