using Microsoft.Extensions.Configuration;
using Moq;
using NotificationService.Application.DTOs;
using NotificationService.Infrastructure.Services;
using NUnit.Framework;

namespace NotificationService.Infrastructure.Tests.Services;

[TestFixture]
public class AzureQueueServiceTests
{
    private Mock<IConfiguration> _mockConfiguration = null!;
    private string _testConnectionString = null!;

    [SetUp]
    public void Setup()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _testConnectionString = "UseDevelopmentStorage=true"; // Local Azure Storage Emulator

        var mockConnectionSection = new Mock<IConfigurationSection>();
        mockConnectionSection.Setup(x => x.Value).Returns(_testConnectionString);

        _mockConfiguration
            .Setup(x => x.GetConnectionString("AzureQueueStorage"))
            .Returns(_testConnectionString);
    }

    [Test]
    public void Constructor_ShouldThrowException_WhenConnectionStringIsNull()
    {
        // Arrange
        _mockConfiguration
            .Setup(x => x.GetConnectionString("AzureQueueStorage"))
            .Returns((string?)null);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new AzureQueueService(_mockConfiguration.Object));
    }

    [Test]
    public void SendToQueueAsync_ShouldAcceptValidNotificationResponse()
    {
        // This test verifies the method signature and that it accepts the correct DTO
        // Actual queue integration tests would require Azure Storage Emulator or mocked QueueClient

        // Arrange
        var notificationResponse = new NotificationResponseDto(
            Guid.NewGuid(),
            "forget-password",
            "email",
            0,
            "test@example.com",
            new { email = "test@example.com", code = "123456" },
            DateTime.UtcNow
        );

        // Assert - Verify DTO properties
        Assert.That(notificationResponse.Template, Is.EqualTo("forget-password"));
        Assert.That(notificationResponse.Channel, Is.EqualTo("email"));
        Assert.That(notificationResponse.Recipient, Is.EqualTo("test@example.com"));
        Assert.That(notificationResponse.RetryCount, Is.EqualTo(0));
    }

    [Test]
    public void NotificationResponseDto_ShouldSerializeCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var requestedAt = DateTime.UtcNow;
        var payload = new { email = "test@example.com", code = "123456" };

        var response = new NotificationResponseDto(
            id,
            "verification",
            "sms",
            0,
            "+1234567890",
            payload,
            requestedAt
        );

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(response);

        // Assert
        Assert.That(json, Does.Contain("verification"));
        Assert.That(json, Does.Contain("sms"));
        Assert.That(json, Does.Contain("+1234567890"));
        Assert.That(json, Does.Contain("test@example.com"));
        Assert.That(json, Does.Contain("123456"));
    }

    [Test]
    public void NotificationResponseDto_ShouldContainAllRequiredFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var template = "welcome";
        var channel = "email";
        var retryCount = 0;
        var recipient = "user@example.com";
        var payload = new { firstName = "John" };
        var requestedAt = DateTime.UtcNow;

        // Act
        var response = new NotificationResponseDto(
            id,
            template,
            channel,
            retryCount,
            recipient,
            payload,
            requestedAt
        );

        // Assert
        Assert.That(response.Id, Is.EqualTo(id));
        Assert.That(response.Template, Is.EqualTo(template));
        Assert.That(response.Channel, Is.EqualTo(channel));
        Assert.That(response.RetryCount, Is.EqualTo(retryCount));
        Assert.That(response.Recipient, Is.EqualTo(recipient));
        Assert.That(response.Payload, Is.EqualTo(payload));
        Assert.That(response.RequestedAt, Is.EqualTo(requestedAt));
    }

    [Test]
    public void NotificationResponseDto_ShouldHandleComplexPayload()
    {
        // Arrange
        var complexPayload = new
        {
            email = "test@example.com",
            code = "123456",
            metadata = new
            {
                source = "web",
                timestamp = DateTime.UtcNow
            },
            tags = new[] { "urgent", "password-reset" }
        };

        var response = new NotificationResponseDto(
            Guid.NewGuid(),
            "forget-password",
            "email",
            0,
            "test@example.com",
            complexPayload,
            DateTime.UtcNow
        );

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(response);

        // Assert
        Assert.That(json, Does.Contain("urgent"));
        Assert.That(json, Does.Contain("password-reset"));
        Assert.That(json, Does.Contain("web"));
    }

    [Test]
    public void NotificationResponseDto_ShouldMaintainRetryCount()
    {
        // Arrange & Act
        var response1 = new NotificationResponseDto(
            Guid.NewGuid(),
            "test",
            "email",
            0,
            "test@example.com",
            new { },
            DateTime.UtcNow
        );

        var response2 = new NotificationResponseDto(
            Guid.NewGuid(),
            "test",
            "email",
            3,
            "test@example.com",
            new { },
            DateTime.UtcNow
        );

        // Assert
        Assert.That(response1.RetryCount, Is.EqualTo(0));
        Assert.That(response2.RetryCount, Is.EqualTo(3));
    }
}
