using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Domain.Repositories;
using System.Text.Json;

namespace NotificationService.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IQueueService queueService) : INotificationService
{
    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        var notif = await notificationRepository.GetByIdAsync(id);
        return notif ?? throw new NotificationNotFoundException("Notification not found.");
    }

    public async Task<NotificationResponseDto?> ProcessNotificationAsync(string template, string channel, string recipient, object payload)
    {
        // Convert payload to JsonDocument
        var payloadJson = JsonSerializer.SerializeToDocument(payload);

        // Create notification
        var notification = new Notification(template, channel, recipient, payloadJson);

        // Insert into database
        await notificationRepository.AddAsync(notification);

        // If SMS or Email, push to Azure Queue
        if (channel == "sms" || channel == "email")
        {
            var response = new NotificationResponseDto(
                notification.Id,
                notification.Template!,
                notification.Channel!,
                notification.RetryCount,
                notification.Recipient!,
                payload,
                notification.RequestedAt
            );

            await queueService.SendToQueueAsync(response);

            // Update status to "pushed"
            await notificationRepository.UpdateStatusAsync(notification.Id, "pushed");

            return response;
        }

        // For in-app notifications, just return the response without pushing to queue
        return null;
    }
}