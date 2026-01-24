using Microsoft.Extensions.Configuration;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Domain.Repositories;

namespace NotificationService.Application.Services;

public class OtpService(
    IOtpRepository otpRepository,
    IOtpFunctionHandler otpFunctionHandler,
    IConfiguration configuration) : IOtpService
{
    public async Task<OtpResponseDto> CreateOtpAsync(CreateOtpRequestDto request)
    {
        // Delete any existing OTPs for this id
        await otpRepository.DeleteByIdAsync(request.Id);

        // Generate a 6-digit code
        var code = GenerateSixDigitCode();

        // Get expiry minutes from configuration
        var expiryMinutes = int.TryParse(configuration["OtpSettings:ExpiryMinutes"], out var mins) ? mins : 60;

        // Create new OTP
        var otp = new Otp(
            request.Id,
            code,
            DateTime.UtcNow.AddMinutes(expiryMinutes)
        );

        // Save to database
        await otpRepository.AddAsync(otp);

        return new OtpResponseDto(
            otp.Id,
            otp.Code,
            otp.CreatedAt,
            otp.ExpiresAt
        );
    }

    public async Task<ValidateOtpResponseDto> ValidateOtpAsync(ValidateOtpRequestDto request)
    {
        // Validate that the function type is valid
        if (!otpFunctionHandler.IsValidFunction(request.Type))
        {
            return new ValidateOtpResponseDto(false, $"Invalid OTP function type: {request.Type}");
        }

        // Get the OTP from database
        var otp = await otpRepository.GetByIdAndCodeAsync(request.Id, request.Code);

        if (otp == null)
        {
            throw new OtpNotFoundException("OTP not found or invalid.");
        }

        // Check if OTP is expired
        if (otp.IsExpired)
        {
            // Delete the expired OTP
            await otpRepository.DeleteByIdAndCodeAsync(request.Id, request.Code);
            throw new OtpExpiredException("OTP has expired.");
        }

        // Delete the OTP (single use)
        await otpRepository.DeleteByIdAndCodeAsync(request.Id, request.Code);

        // Execute the function
        var success = await otpFunctionHandler.ExecuteAsync(request.Type, request.Id);

        if (!success)
        {
            return new ValidateOtpResponseDto(false, "Failed to execute OTP function.");
        }

        return new ValidateOtpResponseDto(true, "OTP validated successfully.");
    }

    public async Task DeleteManyOtpAsync(DeleteManyOtpRequestDto request)
    {
        await otpRepository.DeleteManyByIdsAsync(request.Ids);
    }

    private static string GenerateSixDigitCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
