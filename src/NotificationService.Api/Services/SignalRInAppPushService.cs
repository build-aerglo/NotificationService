using Microsoft.AspNetCore.SignalR;
using NotificationService.Api.Hubs;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Services;

public class SignalRInAppPushService(IHubContext<NotificationHub> hubContext) : IInAppPushService
{
    public Task PushAsync(string recipientId, InAppNotificationDto notification)
    {
        return hubContext.Clients.Group(recipientId).SendAsync("NewNotification", notification);
    }
}
