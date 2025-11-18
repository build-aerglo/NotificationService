using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        try
        {
            var notification = await _notificationService.AddAsync(dto);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, new { error = "Failed to create notification" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNotification(Guid id)
    {
        try
        {
            var notification = await _notificationService.GetByIdAsync(id);
            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification {Id}", id);
            return NotFound(new { error = "Notification not found" });
        }
    }

    /// <summary>
    /// ✅ NEW: Send review approved notification
    /// </summary>
    [HttpPost("review-approved")]
    public async Task<IActionResult> ReviewApproved([FromBody] ReviewNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending review approved notification for review {ReviewId}", request.ReviewId);

            var notification = new CreateNotificationDto(
                NotificationType: "REVIEW_APPROVED",
                NotificationDate: DateTime.UtcNow,
                NotificationStatus: "PENDING",
                MessageHeader: "Your review is live!",
                MessageBody: $"Great news! Your review has been published and is now visible to others. Thank you for sharing your feedback!"
            );

            await _notificationService.AddAsync(notification);
            return Ok(new { message = "Notification created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending review approved notification");
            return StatusCode(500, new { error = "Failed to send notification" });
        }
    }

    /// <summary>
    /// ✅ NEW: Send review rejected notification
    /// </summary>
    [HttpPost("review-rejected")]
    public async Task<IActionResult> ReviewRejected([FromBody] ReviewRejectedNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending review rejected notification for review {ReviewId}", request.ReviewId);

            var reasonsList = string.Join(", ", request.Reasons);
            var notification = new CreateNotificationDto(
                NotificationType: "REVIEW_REJECTED",
                NotificationDate: DateTime.UtcNow,
                NotificationStatus: "PENDING",
                MessageHeader: "Review could not be published",
                MessageBody: $"Unfortunately, your review could not be published for the following reasons: {reasonsList}. Please review our guidelines and try again."
            );

            await _notificationService.AddAsync(notification);
            return Ok(new { message = "Notification created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending review rejected notification");
            return StatusCode(500, new { error = "Failed to send notification" });
        }
    }

    /// <summary>
    /// ✅ NEW: Send review flagged notification
    /// </summary>
    [HttpPost("review-flagged")]
    public async Task<IActionResult> ReviewFlagged([FromBody] ReviewNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending review flagged notification for review {ReviewId}", request.ReviewId);

            var notification = new CreateNotificationDto(
                NotificationType: "REVIEW_FLAGGED",
                NotificationDate: DateTime.UtcNow,
                NotificationStatus: "PENDING",
                MessageHeader: "Your review is under review",
                MessageBody: $"Your review is being reviewed by our moderation team. We'll notify you once it's been approved. This typically takes 24-48 hours."
            );

            await _notificationService.AddAsync(notification);
            return Ok(new { message = "Notification created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending review flagged notification");
            return StatusCode(500, new { error = "Failed to send notification" });
        }
    }
}
