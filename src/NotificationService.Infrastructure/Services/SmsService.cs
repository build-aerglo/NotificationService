using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly SmsSettings _smsSettings;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<SmsService> _logger;
    private readonly HttpClient _httpClient;

    public SmsService(
        IOptions<SmsSettings> smsSettings,
        ITemplateEngine templateEngine,
        ILogger<SmsService> logger,
        HttpClient httpClient)
    {
        _smsSettings = smsSettings?.Value ?? throw new ArgumentNullException(nameof(smsSettings));
        _templateEngine = templateEngine ?? throw new ArgumentNullException(nameof(templateEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            if (_smsSettings.Provider.Equals("Twilio", StringComparison.OrdinalIgnoreCase))
            {
                return await SendViaTwilioAsync(phoneNumber, message);
            }

            _logger.LogWarning("SMS provider {Provider} is not supported", _smsSettings.Provider);
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

    private async Task<bool> SendViaTwilioAsync(string phoneNumber, string message)
    {
        try
        {
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_smsSettings.AccountSid}/Messages.json";

            var formData = new Dictionary<string, string>
            {
                ["To"] = phoneNumber,
                ["From"] = _smsSettings.FromPhoneNumber,
                ["Body"] = message
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new FormUrlEncodedContent(formData)
            };

            // Add basic authentication
            var authToken = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_smsSettings.AccountSid}:{_smsSettings.AuthToken}"));
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
