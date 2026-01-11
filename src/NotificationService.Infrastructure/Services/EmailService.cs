using NotificationService.Application.Interfaces;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly INotificationParamsService _notificationParamsService;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        INotificationParamsService notificationParamsService,
        ITemplateEngine templateEngine,
        ILogger<EmailService> logger)
    {
        _notificationParamsService = notificationParamsService ?? throw new ArgumentNullException(nameof(notificationParamsService));
        _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            // Get SMTP settings from database (with caching)
            var smtpSettings = await _notificationParamsService.GetNotificationParamsAsync();

            if (smtpSettings == null)
            {
                _logger.LogError("SMTP settings not found in database");
                return false;
            }

            using var smtpClient = new SmtpClient(smtpSettings.SmtpHost, smtpSettings.SmtpPort)
            {
                EnableSsl = smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(smtpSettings.SmtpUser, smtpSettings.SmtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.FromEmail, smtpSettings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendForgetPasswordEmailAsync(string email, string code)
    {
        try
        {
            var variables = new Dictionary<string, string>
            {
                ["email"] = email,
                ["code"] = code
            };

            var htmlBody = await _templateEngine.RenderEmailTemplateAsync("forget_password", variables);
            return await SendEmailAsync(email, "Password Reset Request", htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send forget password email to {Email}", email);
            return false;
        }
    }
}
