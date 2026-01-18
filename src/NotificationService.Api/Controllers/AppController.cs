using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/app")]
public class AppController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<AppController> _logger;

    public AppController(
        INotificationService notificationService,
        ILogger<AppController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost("new-message")]
    public async Task<IActionResult> NewMessage([FromQuery] string id, [FromQuery] string message, [FromQuery] string from)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { error = "Id is required" });

            var payload = new { id, message, from };
            await _notificationService.ProcessNotificationAsync(
                "new-message", "in-app", id, payload);

            return Ok(new { message = "In-app notification created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing new-message in-app notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    [HttpPost("review-approved")]
    public async Task<IActionResult> ReviewApproved([FromQuery] string id, [FromQuery] string reviewId)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { error = "Id is required" });

            var payload = new { id, reviewId };
            await _notificationService.ProcessNotificationAsync(
                "review-approved", "in-app", id, payload);

            return Ok(new { message = "In-app notification created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing review-approved in-app notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    [HttpPost("review-rejected")]
    public async Task<IActionResult> ReviewRejected([FromQuery] string id, [FromQuery] string reviewId, [FromQuery] string reason)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { error = "Id is required" });

            var payload = new { id, reviewId, reason };
            await _notificationService.ProcessNotificationAsync(
                "review-rejected", "in-app", id, payload);

            return Ok(new { message = "In-app notification created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing review-rejected in-app notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }
}
