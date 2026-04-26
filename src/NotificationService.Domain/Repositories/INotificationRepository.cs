using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task AddAsync(Notification notification);
    Task UpdateStatusAsync(Guid id, string status, DateTime? deliveredAt = null);
    Task<(IEnumerable<Notification> Items, int Total)> GetInAppByRecipientAsync(string recipientId, int page, int pageSize);
    Task<bool> CloseInAppNotificationAsync(Guid id, string recipientId);
    Task ClearInAppNotificationsByRecipientAsync(string recipientId);
}