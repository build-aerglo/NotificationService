using NotificationService.Application.DTOs;

namespace NotificationService.Application.Interfaces;

public interface IOtpService
{
    Task<OtpResponseDto> CreateOtpAsync(CreateOtpRequestDto request);
    Task<ValidateOtpResponseDto> ValidateOtpAsync(ValidateOtpRequestDto request);
    Task DeleteManyOtpAsync(DeleteManyOtpRequestDto request);
}
