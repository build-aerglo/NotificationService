namespace NotificationService.Infrastructure.Configuration;

public class SmsSettings
{
    public string Provider { get; set; } = "Twilio"; // Default to Twilio
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
}
