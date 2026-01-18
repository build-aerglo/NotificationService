using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/sms")]
public class SmsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<SmsController> _logger;

    public SmsController(
        INotificationService notificationService,
        ILogger<SmsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromQuery] string phone, [FromQuery] string code)
    {
        try
        {
            if (string.IsNullOrEmpty(phone))
                return BadRequest(new { error = "Phone is required" });

            var payload = new { phone, code };
            var response = await _notificationService.ProcessNotificationAsync(
                "forget-password", "sms", phone, payload);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forget-password SMS notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    [HttpPost("verification")]
    public async Task<IActionResult> Verification([FromQuery] string phone, [FromQuery] string code)
    {
        try
        {
            if (string.IsNullOrEmpty(phone))
                return BadRequest(new { error = "Phone is required" });

            var payload = new { phone, code };
            var response = await _notificationService.ProcessNotificationAsync(
                "verification", "sms", phone, payload);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing verification SMS notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }
}
