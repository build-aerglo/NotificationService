using NUnit.Framework;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Services;
using NotificationService.Infrastructure.Configuration;
using NotificationService.Application.Interfaces;
using System.Net;

namespace NotificationService.Infrastructure.Tests.Services;

[TestFixture]
public class SmsServiceTests
{
    private Mock<IOptions<SmsSettings>> _mockSmsSettings = null!;
    private Mock<ITemplateEngine> _mockTemplateEngine = null!;
    private Mock<ILogger<SmsService>> _mockLogger = null!;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private SmsSettings _smsSettings = null!;

    [SetUp]
    public void Setup()
    {
        _smsSettings = new SmsSettings
        {
            Provider = "Twilio",
            AccountSid = "test_account_sid",
            AuthToken = "test_auth_token",
            FromPhoneNumber = "+1234567890"
        };

        _mockSmsSettings = new Mock<IOptions<SmsSettings>>();
        _mockSmsSettings.Setup(x => x.Value).Returns(_smsSettings);

        _mockTemplateEngine = new Mock<ITemplateEngine>();
        _mockLogger = new Mock<ILogger<SmsService>>();

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [Test]
    public void Constructor_WithNullSmsSettings_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SmsService(null!, _mockTemplateEngine.Object, _mockLogger.Object, _httpClient));
    }

    [Test]
    public void Constructor_WithNullTemplateEngine_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SmsService(_mockSmsSettings.Object, null!, _mockLogger.Object, _httpClient));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, null!, _httpClient));
    }

    [Test]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, _mockLogger.Object, null!));
    }

    [Test]
    public async Task SendSmsAsync_WithTwilioProvider_SendsCorrectRequest()
    {
        var phoneNumber = "+1987654321";
        var message = "Test message";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent("{\"sid\":\"test_sid\"}")
            });

        var service = new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, _mockLogger.Object, _httpClient);

        var result = await service.SendSmsAsync(phoneNumber, message);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task SendSmsAsync_WithFailedRequest_ReturnsFalse()
    {
        var phoneNumber = "+1987654321";
        var message = "Test message";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error")
            });

        var service = new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, _mockLogger.Object, _httpClient);

        var result = await service.SendSmsAsync(phoneNumber, message);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SendSmsAsync_WithUnsupportedProvider_ReturnsFalse()
    {
        _smsSettings.Provider = "UnsupportedProvider";

        var service = new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, _mockLogger.Object, _httpClient);

        var result = await service.SendSmsAsync("+1987654321", "Test message");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SendForgetPasswordSmsAsync_CallsTemplateEngineWithCorrectParameters()
    {
        var phoneNumber = "+1987654321";
        var code = "123456";
        var renderedMessage = "Your code is 123456";

        _mockTemplateEngine
            .Setup(x => x.RenderSmsTemplateAsync("forget_password", It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(renderedMessage)
            .Verifiable();

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent("{\"sid\":\"test_sid\"}")
            });

        var service = new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, _mockLogger.Object, _httpClient);

        await service.SendForgetPasswordSmsAsync(phoneNumber, code);

        _mockTemplateEngine.Verify(x => x.RenderSmsTemplateAsync(
            "forget_password",
            It.Is<Dictionary<string, string>>(d => d["code"] == code)),
            Times.Once);
    }

    [Test]
    public async Task SendForgetPasswordSmsAsync_WithTemplateEngineException_ReturnsFalse()
    {
        var phoneNumber = "+1987654321";
        var code = "123456";

        _mockTemplateEngine
            .Setup(x => x.RenderSmsTemplateAsync("forget_password", It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new Exception("Template error"));

        var service = new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, _mockLogger.Object, _httpClient);

        var result = await service.SendForgetPasswordSmsAsync(phoneNumber, code);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SendForgetPasswordSmsAsync_LogsErrorOnException()
    {
        var phoneNumber = "+1987654321";
        var code = "123456";

        _mockTemplateEngine
            .Setup(x => x.RenderSmsTemplateAsync("forget_password", It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new Exception("Template error"));

        var service = new SmsService(_mockSmsSettings.Object, _mockTemplateEngine.Object, _mockLogger.Object, _httpClient);

        await service.SendForgetPasswordSmsAsync(phoneNumber, code);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
