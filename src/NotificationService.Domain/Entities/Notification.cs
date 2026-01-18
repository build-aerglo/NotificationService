using System.Text.Json;

namespace NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string? Template { get; set; }
    public string? Channel { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? Recipient { get; set; }
    public JsonDocument? Payload { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public string Status { get; set; } = "sent";

    public Notification()
    {
        Id = Guid.NewGuid();
        RequestedAt = DateTime.UtcNow;
        Status = "sent";
        RetryCount = 0;
    }

    public Notification(string template, string channel, string recipient, JsonDocument? payload)
    {
        Id = Guid.NewGuid();
        Template = template;
        Channel = channel;
        Recipient = recipient;
        Payload = payload;
        RequestedAt = DateTime.UtcNow;
        Status = "sent";
        RetryCount = 0;
    }
}