using Microsoft.Extensions.Configuration;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

namespace NotificationService.Application.Services;

public class OtpFunctionHandler(
    IBusinessVerificationRepository businessVerificationRepository,
    IPasswordResetRequestRepository passwordResetRequestRepository,
    IConfiguration configuration) : IOtpFunctionHandler
{
    private static readonly HashSet<string> ValidFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "emailVerification",
        "smsVerification",
        "resetPassword"
    };

    public bool IsValidFunction(string functionName)
    {
        return ValidFunctions.Contains(functionName);
    }

    public async Task<bool> ExecuteAsync(string functionName, string id)
    {
        return functionName.ToLowerInvariant() switch
        {
            "emailverification" => await HandleEmailVerificationAsync(id),
            "smsverification" => await HandleSmsVerificationAsync(id),
            "resetpassword" => await HandleResetPasswordAsync(id),
            _ => throw new ArgumentException($"Unknown OTP function: {functionName}")
        };
    }

    private async Task<bool> HandleEmailVerificationAsync(string email)
    {
        var businessId = await businessVerificationRepository.GetBusinessIdByEmailAsync(email);
        if (businessId == null)
        {
            return false;
        }

        await businessVerificationRepository.SetEmailVerifiedAsync(businessId.Value);
        return true;
    }

    private async Task<bool> HandleSmsVerificationAsync(string phone)
    {
        var businessId = await businessVerificationRepository.GetBusinessIdByPhoneAsync(phone);
        if (businessId == null)
        {
            return false;
        }

        await businessVerificationRepository.SetPhoneVerifiedAsync(businessId.Value);
        return true;
    }

    private async Task<bool> HandleResetPasswordAsync(string id)
    {
        // Delete existing password reset requests for this id
        await passwordResetRequestRepository.DeleteByIdAsync(id);

        // Get expiry minutes from configuration
        var expiryMinutes = configuration.GetValue<int>("PasswordResetSettings:ExpiryMinutes", 60);

        // Create a new password reset request
        var passwordResetRequest = new PasswordResetRequest(
            id,
            DateTime.UtcNow.AddMinutes(expiryMinutes)
        );

        await passwordResetRequestRepository.AddAsync(passwordResetRequest);
        return true;
    }
}
