using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Exceptions;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotificationService service, ILogger<NotificationController> logger)
    : ControllerBase
{
    private readonly INotificationService _service = service;
    private readonly ILogger<NotificationController> _logger = logger;
    
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto request)
    {
        try
        {
            var result = await _service.AddAsync(request);
            return CreatedAtAction(nameof(GetNotification), new { id = result.Id }, result);
        }
        catch (NotificationNotFoundException ex)
        {
            _logger.LogWarning(ex, "Notification creation failed: {Message}", ex.Message);
            return Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetNotification(Guid id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }
        catch (NotificationNotFoundException ex)
        {
            _logger.LogWarning(ex, "Notification not found: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
    }
}