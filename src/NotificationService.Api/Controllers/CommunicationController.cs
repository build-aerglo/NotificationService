using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/communication")]
public class CommunicationController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<CommunicationController> _logger;

    public CommunicationController(
        IEmailService emailService,
        ISmsService smsService,
        ILogger<CommunicationController> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends a forget password email
    /// </summary>
    /// <param name="request">Forget password request with email and code</param>
    /// <returns>Success or failure response</returns>
    [HttpPost("forget-password/email")]
    public async Task<IActionResult> SendForgetPasswordEmail([FromBody] ForgetPasswordNotificationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { message = "Code is required" });
            }

            var result = await _emailService.SendForgetPasswordEmailAsync(request.Email, request.Code);

            if (result)
            {
                _logger.LogInformation("Forget password email sent to {Email}", request.Email);
                return Ok(new { message = "Password reset email sent successfully", email = request.Email });
            }

            _logger.LogWarning("Failed to send forget password email to {Email}", request.Email);
            return StatusCode(500, new { message = "Failed to send email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending forget password email to {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred while sending the email" });
        }
    }

    /// <summary>
    /// Sends a forget password SMS
    /// </summary>
    /// <param name="request">Forget password request with phone number and code</param>
    /// <returns>Success or failure response</returns>
    [HttpPost("forget-password/sms")]
    public async Task<IActionResult> SendForgetPasswordSms([FromBody] ForgetPasswordSmsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new { message = "Phone number is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { message = "Code is required" });
            }

            var result = await _smsService.SendForgetPasswordSmsAsync(request.PhoneNumber, request.Code);

            if (result)
            {
                _logger.LogInformation("Forget password SMS sent to {PhoneNumber}", request.PhoneNumber);
                return Ok(new { message = "Password reset SMS sent successfully", phoneNumber = request.PhoneNumber });
            }

            _logger.LogWarning("Failed to send forget password SMS to {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { message = "Failed to send SMS" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending forget password SMS to {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { message = "An error occurred while sending the SMS" });
        }
    }
}
