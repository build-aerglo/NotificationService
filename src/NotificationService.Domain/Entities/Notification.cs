namespace NotificationService.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string? MessageHeader { get; set; }
    public string? MessageBody { get; set; }
    public string NotificationType { get; set; } = default!;
    public DateTime NotificationDate { get; set; }
    public string NotificationStatus { get; set; } = default!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public Notification(string notificationType, string notificationStatus, string? messageBody, string? messageHeader, DateTime notificationDate)
        {
            Id = Guid.NewGuid();
            MessageHeader = messageHeader;
            MessageBody = messageBody;
            NotificationType = notificationType;
            NotificationDate = notificationDate;
            NotificationStatus = notificationStatus;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
}