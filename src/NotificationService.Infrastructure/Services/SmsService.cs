using NotificationService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace NotificationService.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly INotificationParamsService _notificationParamsService;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<SmsService> _logger;
    private readonly HttpClient _httpClient;

    public SmsService(
        INotificationParamsService notificationParamsService,
        ITemplateEngine templateEngine,
        ILogger<SmsService> logger,
        HttpClient httpClient)
    {
        _notificationParamsService = notificationParamsService ?? throw new ArgumentNullException(nameof(notificationParamsService));
        _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // Get SMS settings from database (with caching)
            var smsSettings = await _notificationParamsService.GetNotificationParamsAsync();

            if (smsSettings == null)
            {
                _logger.LogError("SMS settings not found in database");
                return false;
            }

            if (smsSettings.SmsProvider.Equals("Twilio", StringComparison.OrdinalIgnoreCase))
            {
                return await SendViaTwilioAsync(phoneNumber, message, smsSettings);
            }

            _logger.LogWarning("SMS provider {Provider} is not supported", smsSettings.SmsProvider);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendForgetPasswordSmsAsync(string phoneNumber, string code)
    {
        try
        {
            var variables = new Dictionary<string, string>
            {
                ["code"] = code
            };

            var message = await _templateEngine.RenderSmsTemplateAsync("forget_password", variables);
            return await SendSmsAsync(phoneNumber, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send forget password SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    private async Task<bool> SendViaTwilioAsync(string phoneNumber, string message, Domain.Entities.NotificationParams smsSettings)
    {
        try
        {
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{smsSettings.SmsAccountSid}/Messages.json";

            var formData = new Dictionary<string, string>
            {
                ["To"] = phoneNumber,
                ["From"] = smsSettings.SmsFromNumber,
                ["Body"] = message
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(formData)
            };

            // Add basic authentication
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{smsSettings.SmsAccountSid}:{smsSettings.SmsAuthToken}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber} via Twilio", phoneNumber);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to send SMS via Twilio. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Twilio to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}
