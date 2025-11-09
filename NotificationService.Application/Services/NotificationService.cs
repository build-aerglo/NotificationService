using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Domain.Repositories;


namespace NotificationService.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository): INotificationService
{
    public async Task<Notification?> GetByIdAsync(Guid id)
    {
       var notif = await notificationRepository.GetByIdAsync(id);
       return notif ?? throw new NotificationNotFoundException("Notification not found.");
    }

    public async Task<Notification> AddAsync(CreateNotificationDto notification)
    {
        var notif = new Notification(notification.NotificationType, notification.NotificationStatus, notification.MessageBody, notification.MessageHeader, notification.NotificationDate);
        await notificationRepository.AddAsync(notif);
        
        // validate 
        var notificationCreated = await GetByIdAsync(notif.Id);
        return notificationCreated != null ? throw new NotificationNotFoundException("Error creating notification.") : notif;
    }
}