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
    JsonElement Payload
);

public record InAppNotificationDto(
    Guid Id,
    string? Template,
    JsonDocument? Payload,
    DateTime RequestedAt
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int Total,
    bool HasNext
);

public record CloseNotificationRequestDto(string RecipientId);

public record ClearNotificationsRequestDto(string RecipientId);