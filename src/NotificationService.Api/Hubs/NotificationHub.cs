using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Api.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var recipientId = Context.GetHttpContext()?.Request.Query["recipientId"].ToString();
        if (!string.IsNullOrWhiteSpace(recipientId))
            await Groups.AddToGroupAsync(Context.ConnectionId, recipientId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var recipientId = Context.GetHttpContext()?.Request.Query["recipientId"].ToString();
        if (!string.IsNullOrWhiteSpace(recipientId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, recipientId);
        await base.OnDisconnectedAsync(exception);
    }
}
