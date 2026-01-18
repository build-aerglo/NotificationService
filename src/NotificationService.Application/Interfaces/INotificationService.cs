using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<NotificationResponseDto?> ProcessNotificationAsync(string template, string channel, string recipient, object payload);
}