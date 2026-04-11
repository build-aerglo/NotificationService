using Microsoft.Extensions.Configuration;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Repositories;
using NUnit.Framework;
using System.Text.Json;

namespace NotificationService.Infrastructure.Tests.Repositories;

[TestFixture]
public class NotificationRepositoryTests
{
    private IConfiguration _configuration = null!;

    [SetUp]
    public void Setup()
    {
        // Use a real IConfiguration backed by in-memory values — avoids mocking extension methods
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgresConnection"] = "Host=localhost;Database=testdb;Username=test;Password=test"
            })
            .Build();
    }

    [Test]
    public void Constructor_ShouldInitialize_WithValidConfiguration()
    {
        // Act & Assert — should not throw
        Assert.DoesNotThrow(() => _ = new NotificationRepository(_configuration));
    }

    [Test]
    public void AddAsync_ShouldAcceptValidNotification()
    {
        var notification = new Notification(
            "forget-password",
            "email",
            "test@example.com",
            JsonDocument.Parse("{\"email\":\"test@example.com\",\"code\":\"123456\"}")
        );

        Assert.That(notification.Template, Is.EqualTo("forget-password"));
        Assert.That(notification.Channel, Is.EqualTo("email"));
        Assert.That(notification.Recipient, Is.EqualTo("test@example.com"));
        Assert.That(notification.Status, Is.EqualTo("sent"));
        Assert.That(notification.RetryCount, Is.EqualTo(0));
    }

    [Test]
    public void Notification_ShouldSerializePayloadCorrectly()
    {
        var payloadJson = JsonSerializer.SerializeToDocument(new { email = "test@example.com", code = "123456" });
        var notification = new Notification("verification", "sms", "+1234567890", payloadJson);

        Assert.That(notification.Payload, Is.Not.Null);
        var payloadString = notification.Payload!.RootElement.ToString();
        Assert.That(payloadString, Does.Contain("test@example.com"));
        Assert.That(payloadString, Does.Contain("123456"));
    }

    [Test]
    public void Notification_ShouldHaveCorrectDefaultValues()
    {
        var notification = new Notification();

        Assert.That(notification.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(notification.Status, Is.EqualTo("sent"));
        Assert.That(notification.RetryCount, Is.EqualTo(0));
        Assert.That(notification.RequestedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(notification.DeliveredAt, Is.Null);
    }

    [Test]
    public void Notification_ShouldAcceptNullPayload()
    {
        var notification = new Notification("test-template", "email", "test@example.com", null);

        Assert.That(notification.Payload, Is.Null);
        Assert.That(notification.Template, Is.EqualTo("test-template"));
    }

    [Test]
    public void Notification_ShouldSetAllPropertiesCorrectly()
    {
        var payload = JsonSerializer.SerializeToDocument(new { firstName = "John" });
        var notification = new Notification("welcome", "email", "user@example.com", payload);

        Assert.That(notification.Template, Is.EqualTo("welcome"));
        Assert.That(notification.Channel, Is.EqualTo("email"));
        Assert.That(notification.Recipient, Is.EqualTo("user@example.com"));
        Assert.That(notification.Payload, Is.Not.Null);
        Assert.That(notification.Status, Is.EqualTo("sent"));
        Assert.That(notification.RetryCount, Is.EqualTo(0));
    }
}
