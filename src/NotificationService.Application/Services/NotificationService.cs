using NotificationService.Application.Configuration;
using NotificationService.Application.Constants;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Domain.Repositories;
using System.Text.Json;

namespace NotificationService.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IQueueService queueService,
    IInAppPushService inAppPushService) : INotificationService
{
    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        var notif = await notificationRepository.GetByIdAsync(id);
        return notif ?? throw new NotificationNotFoundException("Notification not found.");
    }

    public async Task<NotificationResponseDto?> ProcessNotificationAsync(string template, string channel, string recipient, object payload)
    {
        var payloadJson = JsonSerializer.SerializeToDocument(payload);
        var notification = new Notification(template, channel, recipient, payloadJson);

        var skipDbSave = SkipDbSaveTemplates.Templates.Contains(template);

        if (!skipDbSave)
        {
            await notificationRepository.AddAsync(notification);
        }

        if (channel == NotificationChannels.Sms || channel == NotificationChannels.Email)
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

            if (!skipDbSave)
            {
                await notificationRepository.UpdateStatusAsync(notification.Id, "pushed");
            }

            return response;
        }

        if (channel == NotificationChannels.InApp)
        {
            var inAppDto = new InAppNotificationDto(
                notification.Id,
                notification.Template,
                notification.Payload,
                notification.RequestedAt
            );
            await inAppPushService.PushAsync(recipient, inAppDto);
        }

        return null;
    }

    public async Task<PagedResult<InAppNotificationDto>> GetInAppNotificationsAsync(
        string recipientId, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await notificationRepository.GetInAppByRecipientAsync(recipientId, page, pageSize);

        var dtos = items.Select(n => new InAppNotificationDto(
            n.Id,
            n.Template,
            n.Payload,
            n.RequestedAt
        ));

        return new PagedResult<InAppNotificationDto>(
            dtos,
            page,
            pageSize,
            total,
            page * pageSize < total
        );
    }

    public async Task CloseNotificationAsync(Guid notificationId, string recipientId)
    {
        var updated = await notificationRepository.CloseInAppNotificationAsync(notificationId, recipientId);
        if (!updated)
            throw new NotificationNotFoundException();
    }

    public async Task ClearNotificationsAsync(string recipientId)
    {
        await notificationRepository.ClearInAppNotificationsByRecipientAsync(recipientId);
    }
}
