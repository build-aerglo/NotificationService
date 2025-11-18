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

//  Request DTOs for review notifications
public record ReviewNotificationRequest(
    string Email, 
    Guid ReviewId
);

public record ReviewRejectedNotificationRequest(
    string Email, 
    Guid ReviewId, 
    List<string> Reasons
);