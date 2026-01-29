using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/otp")]
public class OtpController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly ILogger<OtpController> _logger;

    public OtpController(
        IOtpService otpService,
        ILogger<OtpController> logger)
    {
        _otpService = otpService;
        _logger = logger;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOtp([FromQuery] string id, [FromQuery] string type, [FromQuery] string purpose)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { error = "Id is required" });

            if (string.IsNullOrEmpty(type))
                return BadRequest(new { error = "Type is required" });

            if (type != "email" && type != "sms")
                return BadRequest(new { error = "Type must be 'email' or 'sms'" });

            if (string.IsNullOrEmpty(purpose))
                return BadRequest(new { error = "Purpose is required" });

            var request = new CreateOtpRequestDto(id, type, purpose);
            var response = await _otpService.CreateOtpAsync(request);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OTP");
            return StatusCode(500, new { error = "Failed to create OTP" });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateOtp([FromQuery] string id, [FromQuery] string code, [FromQuery] string type)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { error = "Id is required" });

            if (string.IsNullOrEmpty(code))
                return BadRequest(new { error = "Code is required" });

            if (string.IsNullOrEmpty(type))
                return BadRequest(new { error = "Type is required" });

            var request = new ValidateOtpRequestDto(id, code, type);
            var response = await _otpService.ValidateOtpAsync(request);

            return Ok(response);
        }
        catch (OtpNotFoundException)
        {
            return NotFound(new { error = "OTP not found or invalid" });
        }
        catch (OtpExpiredException)
        {
            return BadRequest(new { error = "OTP has expired" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OTP");
            return StatusCode(500, new { error = "Failed to validate OTP" });
        }
    }

    [HttpDelete("delete-many")]
    public async Task<IActionResult> DeleteManyOtp([FromBody] DeleteManyOtpRequestDto request)
    {
        try
        {
            if (request.Ids == null || !request.Ids.Any())
                return BadRequest(new { error = "Ids are required" });

            await _otpService.DeleteManyOtpAsync(request);

            return Ok(new { message = "OTPs deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting OTPs");
            return StatusCode(500, new { error = "Failed to delete OTPs" });
        }
    }
}
