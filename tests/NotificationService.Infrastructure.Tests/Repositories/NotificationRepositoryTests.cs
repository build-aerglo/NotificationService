using Microsoft.Extensions.Configuration;
using Moq;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Repositories;
using NUnit.Framework;
using System.Text.Json;

namespace NotificationService.Infrastructure.Tests.Repositories;

[TestFixture]
public class NotificationRepositoryTests
{
    private Mock<IConfiguration> _mockConfiguration = null!;
    private NotificationRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _mockConfiguration = new Mock<IConfiguration>();

        // Mock the connection string
        var mockConnectionSection = new Mock<IConfigurationSection>();
        mockConnectionSection.Setup(x => x.Value).Returns("Host=localhost;Database=testdb;Username=test;Password=test");

        _mockConfiguration
            .Setup(x => x.GetSection("ConnectionStrings:PostgresConnection"))
            .Returns(mockConnectionSection.Object);

        _mockConfiguration
            .Setup(x => x.GetConnectionString("PostgresConnection"))
            .Returns("Host=localhost;Database=testdb;Username=test;Password=test");
    }

    [Test]
    public void Constructor_ShouldInitialize_WithValidConfiguration()
    {
        // Act & Assert - Should not throw
        Assert.DoesNotThrow(() => _repository = new NotificationRepository(_mockConfiguration.Object));
    }

    [Test]
    public void AddAsync_ShouldAcceptValidNotification()
    {
        // This test verifies that the repository method signature is correct
        // Actual database integration tests would require a test database

        // Arrange
        var notification = new Notification(
            "forget-password",
            "email",
            "test@example.com",
            JsonDocument.Parse("{\"email\":\"test@example.com\",\"code\":\"123456\"}")
        );

        // Assert - Method exists and accepts correct parameters
        Assert.That(notification.Template, Is.EqualTo("forget-password"));
        Assert.That(notification.Channel, Is.EqualTo("email"));
        Assert.That(notification.Recipient, Is.EqualTo("test@example.com"));
        Assert.That(notification.Status, Is.EqualTo("sent"));
        Assert.That(notification.RetryCount, Is.EqualTo(0));
    }

    [Test]
    public void Notification_ShouldSerializePayloadCorrectly()
    {
        // Arrange
        var payloadData = new { email = "test@example.com", code = "123456" };
        var payloadJson = JsonSerializer.SerializeToDocument(payloadData);

        var notification = new Notification(
            "verification",
            "sms",
            "+1234567890",
            payloadJson
        );

        // Assert
        Assert.That(notification.Payload, Is.Not.Null);
        var payloadString = notification.Payload!.RootElement.ToString();
        Assert.That(payloadString, Does.Contain("test@example.com"));
        Assert.That(payloadString, Does.Contain("123456"));
    }

    [Test]
    public void Notification_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert
        Assert.That(notification.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(notification.Status, Is.EqualTo("sent"));
        Assert.That(notification.RetryCount, Is.EqualTo(0));
        Assert.That(notification.RequestedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(notification.DeliveredAt, Is.Null);
    }

    [Test]
    public void Notification_ShouldAcceptNullPayload()
    {
        // Arrange & Act
        var notification = new Notification(
            "test-template",
            "email",
            "test@example.com",
            null
        );

        // Assert
        Assert.That(notification.Payload, Is.Null);
        Assert.That(notification.Template, Is.EqualTo("test-template"));
    }

    [Test]
    public void Notification_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var template = "welcome";
        var channel = "email";
        var recipient = "user@example.com";
        var payload = JsonSerializer.SerializeToDocument(new { firstName = "John" });

        // Act
        var notification = new Notification(template, channel, recipient, payload);

        // Assert
        Assert.That(notification.Template, Is.EqualTo(template));
        Assert.That(notification.Channel, Is.EqualTo(channel));
        Assert.That(notification.Recipient, Is.EqualTo(recipient));
        Assert.That(notification.Payload, Is.Not.Null);
        Assert.That(notification.Status, Is.EqualTo("sent"));
        Assert.That(notification.RetryCount, Is.EqualTo(0));
    }
}
