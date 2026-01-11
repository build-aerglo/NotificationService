using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Services;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Tests.Services;

[TestFixture]
public class EmailServiceTests
{
    private Mock<INotificationParamsService> _mockNotificationParamsService = null!;
    private Mock<ITemplateEngine> _mockTemplateEngine = null!;
    private Mock<ILogger<EmailService>> _mockLogger = null!;
    private NotificationParams _notificationParams = null!;

    [SetUp]
    public void Setup()
    {
        _notificationParams = new NotificationParams
        {
            Id = 1,
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUser = "test@test.com",
            SmtpPassword = "password",
            EnableSsl = true,
            FromEmail = "noreply@test.com",
            FromName = "Test Service",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockNotificationParamsService = new Mock<INotificationParamsService>();
        _mockNotificationParamsService
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync(_notificationParams);

        _mockTemplateEngine = new Mock<ITemplateEngine>();
        _mockLogger = new Mock<ILogger<EmailService>>();
    }

    [Test]
    public void Constructor_WithNullNotificationParamsService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmailService(null!, _mockTemplateEngine.Object, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullTemplateEngine_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmailService(_mockNotificationParamsService.Object, null!, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EmailService(_mockNotificationParamsService.Object, _mockTemplateEngine.Object, null!));
    }

    [Test]
    public async Task SendForgetPasswordEmailAsync_CallsTemplateEngineWithCorrectParameters()
    {
        var email = "user@test.com";
        var code = "123456";
        var expectedHtml = "<html>Test email</html>";

        _mockTemplateEngine
            .Setup(x => x.RenderEmailTemplateAsync("forget_password", It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedHtml)
            .Verifiable();

        var service = new EmailService(_mockNotificationParamsService.Object, _mockTemplateEngine.Object, _mockLogger.Object);

        // Note: This will attempt to send email which will fail in test environment
        // We're primarily testing that template engine is called correctly
        await service.SendForgetPasswordEmailAsync(email, code);

        _mockTemplateEngine.Verify(x => x.RenderEmailTemplateAsync(
            "forget_password",
            It.Is<Dictionary<string, string>>(d =>
                d["email"] == email && d["code"] == code)),
            Times.Once);
    }

    [Test]
    public async Task SendForgetPasswordEmailAsync_WithTemplateEngineException_ReturnsFalse()
    {
        var email = "user@test.com";
        var code = "123456";

        _mockTemplateEngine
            .Setup(x => x.RenderEmailTemplateAsync("forget_password", It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new Exception("Template error"));

        var service = new EmailService(_mockNotificationParamsService.Object, _mockTemplateEngine.Object, _mockLogger.Object);

        var result = await service.SendForgetPasswordEmailAsync(email, code);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SendForgetPasswordEmailAsync_LogsErrorOnException()
    {
        var email = "user@test.com";
        var code = "123456";

        _mockTemplateEngine
            .Setup(x => x.RenderEmailTemplateAsync("forget_password", It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new Exception("Template error"));

        var service = new EmailService(_mockNotificationParamsService.Object, _mockTemplateEngine.Object, _mockLogger.Object);

        await service.SendForgetPasswordEmailAsync(email, code);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Test]
    public async Task SendEmailAsync_WithNoNotificationParams_ReturnsFalse()
    {
        _mockNotificationParamsService
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync((NotificationParams?)null);

        var service = new EmailService(_mockNotificationParamsService.Object, _mockTemplateEngine.Object, _mockLogger.Object);

        var result = await service.SendEmailAsync("test@test.com", "Test Subject", "<html>Test</html>");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task SendEmailAsync_WithNoNotificationParams_LogsError()
    {
        _mockNotificationParamsService
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync((NotificationParams?)null);

        var service = new EmailService(_mockNotificationParamsService.Object, _mockTemplateEngine.Object, _mockLogger.Object);

        await service.SendEmailAsync("test@test.com", "Test Subject", "<html>Test</html>");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SMTP settings not found")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}
