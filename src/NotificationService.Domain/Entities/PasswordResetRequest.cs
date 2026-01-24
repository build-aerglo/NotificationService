namespace NotificationService.Domain.Entities;

public class PasswordResetRequest
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public PasswordResetRequest()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public PasswordResetRequest(string id, DateTime expiresAt)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
