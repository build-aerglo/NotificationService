using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/otp")]
[Authorize]
[EnableRateLimiting("otp")]
public partial class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ILogger<OtpController> _logger;

    // Compile-time regex for 6-digit OTP code validation
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex SixDigitCodeRegex();

    public OtpController(IOtpService otpService, ILogger<OtpController> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOtp(
        [FromQuery] string id,
        [FromQuery] string type,
        [FromQuery] string purpose)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 255)
                return BadRequest(new { error = "Id is required and must not exceed 255 characters." });

            if (string.IsNullOrWhiteSpace(type))
                return BadRequest(new { error = "Type is required." });

            if (type != "email" && type != "sms")
                return BadRequest(new { error = "Type must be 'email' or 'sms'." });

            if (string.IsNullOrWhiteSpace(purpose) || purpose.Length > 100)
                return BadRequest(new { error = "Purpose is required and must not exceed 100 characters." });

            _logger.LogInformation(
                "Creating OTP. Type: {Type}, Purpose: {Purpose}. TraceId: {TraceId}",
                type, purpose, HttpContext?.TraceIdentifier);

            var request = new CreateOtpRequestDto(id, type, purpose);
            var response = await _otpService.CreateOtpAsync(request);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OTP. TraceId: {TraceId}", HttpContext?.TraceIdentifier);
            return StatusCode(500, new { error = "Failed to create OTP" });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateOtp(
        [FromQuery] string id,
        [FromQuery] string code,
        [FromQuery] string type)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 255)
                return BadRequest(new { error = "Id is required and must not exceed 255 characters." });

            if (string.IsNullOrWhiteSpace(code) || !SixDigitCodeRegex().IsMatch(code))
                return BadRequest(new { error = "Code must be exactly 6 digits." });

            if (string.IsNullOrWhiteSpace(type) || type.Length > 50)
                return BadRequest(new { error = "Type is required and must not exceed 50 characters." });

            _logger.LogInformation(
                "Validating OTP. Type: {Type}. TraceId: {TraceId}",
                type, HttpContext?.TraceIdentifier);

            var request = new ValidateOtpRequestDto(id, code, type);
            var response = await _otpService.ValidateOtpAsync(request);
            return Ok(response);
        }
        catch (OtpNotFoundException)
        {
            return NotFound(new { error = "OTP not found or invalid." });
        }
        catch (OtpExpiredException)
        {
            return BadRequest(new { error = "OTP has expired." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OTP. TraceId: {TraceId}", HttpContext?.TraceIdentifier);
            return StatusCode(500, new { error = "Failed to validate OTP" });
        }
    }

    [HttpDelete("delete-many")]
    [EnableRateLimiting("general")]
    public async Task<IActionResult> DeleteManyOtp([FromBody] DeleteManyOtpRequestDto request)
    {
        try
        {
            if (request.Ids == null || !request.Ids.Any())
                return BadRequest(new { error = "Ids are required." });

            var ids = request.Ids.ToList();

            if (ids.Count > 100)
                return BadRequest(new { error = "Cannot delete more than 100 OTPs in a single request." });

            if (ids.Any(id => string.IsNullOrWhiteSpace(id) || id.Length > 255))
                return BadRequest(new { error = "Each id must be a non-empty string of at most 255 characters." });

            _logger.LogInformation(
                "Deleting {Count} OTPs. TraceId: {TraceId}",
                ids.Count, HttpContext?.TraceIdentifier);

            await _otpService.DeleteManyOtpAsync(request);

            return Ok(new { message = "OTPs deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting OTPs. TraceId: {TraceId}", HttpContext?.TraceIdentifier);
            return StatusCode(500, new { error = "Failed to delete OTPs" });
        }
    }
}
