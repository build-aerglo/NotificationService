namespace NotificationService.Application.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Sends an email with HTML body
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="htmlBody">HTML body content</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody);

    /// <summary>
    /// Sends a forget password email using template
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="code">Reset code</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendForgetPasswordEmailAsync(string email, string code);
}
