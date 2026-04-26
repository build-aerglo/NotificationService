using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface IInAppPushService
{
    Task PushAsync(string recipientId, InAppNotificationDto notification);
}
