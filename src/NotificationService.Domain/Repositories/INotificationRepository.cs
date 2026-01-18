using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task AddAsync(Notification notification);
    Task UpdateStatusAsync(Guid id, string status, DateTime? deliveredAt = null);
}