namespace NotificationService.Application.DTOs;

public record NotificationDto(
    Guid Id,
    string NotificationType,
    DateTime NotificationDate,
    string NotificationStatus,
    string? MessageHeader,
    string? MessageBody);

public record CreateNotificationDto(
    string NotificationType,
    DateTime NotificationDate,
    string NotificationStatus,
    string? MessageHeader,
    string? MessageBody
    );