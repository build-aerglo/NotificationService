namespace NotificationService.Domain.Entities;

public class Otp
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public Otp()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public Otp(string id, string code, DateTime expiresAt)
    {
        Id = id;
        Code = code;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
