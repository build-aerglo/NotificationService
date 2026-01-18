using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        INotificationService notificationService,
        ILogger<EmailController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromQuery] string email, [FromQuery] string code)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email is required" });

            var payload = new { email, code };
            var response = await _notificationService.ProcessNotificationAsync(
                "forget-password", "email", email, payload);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forget-password email notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    [HttpPost("welcome")]
    public async Task<IActionResult> Welcome([FromQuery] string email, [FromQuery] string firstName)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email is required" });

            var payload = new { email, firstName };
            var response = await _notificationService.ProcessNotificationAsync(
                "welcome", "email", email, payload);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing welcome email notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    [HttpPost("order-confirmation")]
    public async Task<IActionResult> OrderConfirmation([FromQuery] string email, [FromQuery] string orderId, [FromQuery] string orderTotal)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "Email is required" });

            var payload = new { email, orderId, orderTotal };
            var response = await _notificationService.ProcessNotificationAsync(
                "order-confirmation", "email", email, payload);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order-confirmation email notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }
}
