namespace NotificationService.Application.DTOs;

public record CreateOtpRequestDto(
    string Id,
    string Type,
    string Purpose
);

public record ValidateOtpRequestDto(
    string Id,
    string Code,
    string Type
);

public record DeleteManyOtpRequestDto(
    IEnumerable<string> Ids
);

public record OtpResponseDto(
    string Id,
    string Code,
    DateTime CreatedAt,
    DateTime ExpiresAt
);

public record ValidateOtpResponseDto(
    bool Success,
    string Message
);
