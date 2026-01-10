using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace NotificationService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpSettings> smtpSettings,
        ITemplateEngine templateEngine,
        ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings?.Value ?? throw new ArgumentNullException(nameof(smtpSettings));
        _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
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
