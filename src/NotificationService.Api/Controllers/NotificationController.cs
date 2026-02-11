using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/notification")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequestDto request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Template))
                return BadRequest(new { error = "Template is required" });

            if (string.IsNullOrEmpty(request.Channel))
                return BadRequest(new { error = "Channel is required" });

            if (string.IsNullOrEmpty(request.Recipient))
                return BadRequest(new { error = "Recipient is required" });

            if (request.Payload == null)
                return BadRequest(new { error = "Payload is required" });

            var response = await _notificationService.ProcessNotificationAsync(
                request.Template, request.Channel, request.Recipient, request.Payload);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }
}
