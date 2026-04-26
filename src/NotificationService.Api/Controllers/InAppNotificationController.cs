using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/in-app-notifications")]
[Authorize]
[EnableRateLimiting("general")]
public class InAppNotificationController(
    INotificationService notificationService,
    ILogger<InAppNotificationController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] string recipientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(recipientId))
            return BadRequest(new { error = "recipientId is required" });

        logger.LogInformation(
            "Fetching in-app notifications for recipient {RecipientId}, page {Page}. TraceId: {TraceId}",
            recipientId, page, HttpContext.TraceIdentifier);

        var result = await notificationService.GetInAppNotificationsAsync(recipientId, page, pageSize);
        return Ok(result);
    }

    [HttpPut("{notificationId:guid}/close")]
    public async Task<IActionResult> CloseNotification(
        Guid notificationId,
        [FromBody] CloseNotificationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientId))
            return BadRequest(new { error = "recipientId is required" });

        await notificationService.CloseNotificationAsync(notificationId, request.RecipientId);
        return NoContent();
    }

    [HttpPut("clear")]
    public async Task<IActionResult> ClearNotifications([FromBody] ClearNotificationsRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientId))
            return BadRequest(new { error = "recipientId is required" });

        await notificationService.ClearNotificationsAsync(request.RecipientId);
        return NoContent();
    }
}
