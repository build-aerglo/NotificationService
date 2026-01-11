namespace NotificationService.Domain.Entities;

public class NotificationParams
{
    public int Id { get; set; }

    // SMTP/Email settings
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;

    // SMS settings
    public string SmsProvider { get; set; } = string.Empty;
    public string SmsAccountSid { get; set; } = string.Empty;
    public string SmsAuthToken { get; set; } = string.Empty;
    public string SmsFromNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
