using System.Text.Json;

namespace NotificationService.Application.DTOs;

public record NotificationDto(
    Guid Id,
    string? Template,
    string? Channel,
    int RetryCount,
    string? Recipient,
    JsonDocument? Payload,
    DateTime RequestedAt,
    DateTime? DeliveredAt,
    string Status
);

public record NotificationResponseDto(
    Guid Id,
    string Template,
    string Channel,
    int RetryCount,
    string Recipient,
    object Payload,
    DateTime RequestedAt
);

public record CreateNotificationRequestDto(
    string Template,
    string Channel,
    string Recipient,
    object Payload
);