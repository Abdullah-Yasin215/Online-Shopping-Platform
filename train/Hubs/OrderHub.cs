using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace train.Hubs
{
    [Authorize(Policy = "AdminOnly")]
    public class OrderHub : Hub
    {
        public const string GroupName = "Admins";

        public override async Task OnConnectedAsync()
        {
            // Add the connected admin to the "Admins" group
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
            await base.OnConnectedAsync();
        }
    }
}
