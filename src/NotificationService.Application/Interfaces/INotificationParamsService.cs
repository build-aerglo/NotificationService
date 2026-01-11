using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationParamsService
{
    /// <summary>
    /// Gets the notification parameters (SMTP settings) with caching
    /// </summary>
    /// <returns>The notification parameters or null if not found</returns>
    Task<NotificationParams?> GetNotificationParamsAsync();

    /// <summary>
    /// Clears the cache for notification parameters
    /// </summary>
    void ClearCache();
}
