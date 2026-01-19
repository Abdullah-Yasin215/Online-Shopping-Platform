using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

public class CartHub : Hub
{

    // Create a stable group key for the user or session
    //public static string GroupKey(string? userId, string sessionId)
    //{
    //    if (!string.IsNullOrEmpty(userId)) return $"u:{userId}";
    //    return string.IsNullOrEmpty(sessionId) ? "" : $"g:{sessionId}";
    //}
    public static string GroupKey(string? userId, string? sid)
    {
        return userId != null ? $"user:{userId}" : $"session:{sid}";
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = Context.GetHttpContext()?.Request.Cookies["sf_sid"];
        var key = GroupKey(userId, sessionId);

        if (!string.IsNullOrEmpty(key))
            await Groups.AddToGroupAsync(Context.ConnectionId, key);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionId = Context.GetHttpContext()?.Request.Cookies["sf_sid"];
        var key = GroupKey(userId, sessionId);

        if (!string.IsNullOrEmpty(key))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, key);

        await base.OnDisconnectedAsync(exception);
    }
}
