using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;

namespace train.Hubs
{
    [Authorize(Policy = "AdminOnly")]
    [Route("/hubs/admin")] // ✅ ADD THIS LINE - CRITICAL!
    public class AdminHub : Hub
    {
        public const string GroupName = "Admins";

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context.User?.Identity?.Name;
            var userEmail = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            // Log this information to debug
            Console.WriteLine($"AdminHub connected: UserId={userId}, UserName={userName}, Email={userEmail}");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
            await base.OnConnectedAsync();
        }
    }
}