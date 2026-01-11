using NotificationService.Domain.Entities;

namespace NotificationService.Domain.Repositories;

public interface INotificationParamsRepository
{
    /// <summary>
    /// Gets the notification parameters (SMTP settings)
    /// </summary>
    /// <returns>The notification parameters or null if not found</returns>
    Task<NotificationParams?> GetNotificationParamsAsync();
}
