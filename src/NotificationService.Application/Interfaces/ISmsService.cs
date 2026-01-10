namespace NotificationService.Application.Interfaces;

public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number (E.164 format recommended)</param>
    /// <param name="message">SMS message content</param>
    /// <returns>True if SMS sent successfully, false otherwise</returns>
    Task<bool> SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Sends a forget password SMS using template
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="code">Reset code</param>
    /// <returns>True if SMS sent successfully, false otherwise</returns>
    Task<bool> SendForgetPasswordSmsAsync(string phoneNumber, string code);
}
