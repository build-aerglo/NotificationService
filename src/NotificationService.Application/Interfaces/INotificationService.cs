using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<NotificationResponseDto?> ProcessNotificationAsync(string template, string channel, string recipient, object payload);
    Task<PagedResult<InAppNotificationDto>> GetInAppNotificationsAsync(string recipientId, int page, int pageSize);
    Task CloseNotificationAsync(Guid notificationId, string recipientId);
    Task ClearNotificationsAsync(string recipientId);
}