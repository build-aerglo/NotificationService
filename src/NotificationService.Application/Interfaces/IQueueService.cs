using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface IQueueService
{
    Task SendToQueueAsync(NotificationResponseDto notification);
}
