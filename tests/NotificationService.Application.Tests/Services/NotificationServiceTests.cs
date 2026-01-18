using Moq;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using NUnit.Framework;
using System.Text.Json;

namespace NotificationService.Application.Tests.Services;

[TestFixture]
public class NotificationServiceTests
{
    private Mock<INotificationRepository> _mockRepository = null!;
    private Mock<IQueueService> _mockQueueService = null!;
    private NotificationService.Application.Services.NotificationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<INotificationRepository>();
        _mockQueueService = new Mock<IQueueService>();
        _service = new NotificationService.Application.Services.NotificationService(
            _mockRepository.Object,
            _mockQueueService.Object);
    }

    [Test]
    public async Task ProcessNotificationAsync_ShouldCreateNotification_ForSms()
    {
        // Arrange
        var template = "forget-password";
        var channel = "sms";
        var recipient = "+1234567890";
        var payload = new { phone = recipient, code = "123456" };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.UpdateStatusAsync(It.IsAny<Guid>(), "pushed", null))
            .Returns(Task.CompletedTask);

        _mockQueueService
            .Setup(q => q.SendToQueueAsync(It.IsAny<DTOs.NotificationResponseDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessNotificationAsync(template, channel, recipient, payload);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Template, Is.EqualTo(template));
        Assert.That(result.Channel, Is.EqualTo(channel));
        Assert.That(result.Recipient, Is.EqualTo(recipient));

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        _mockQueueService.Verify(q => q.SendToQueueAsync(It.IsAny<DTOs.NotificationResponseDto>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), "pushed", null), Times.Once);
    }

    [Test]
    public async Task ProcessNotificationAsync_ShouldCreateNotification_ForEmail()
    {
        // Arrange
        var template = "welcome";
        var channel = "email";
        var recipient = "test@example.com";
        var payload = new { email = recipient, firstName = "John" };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.UpdateStatusAsync(It.IsAny<Guid>(), "pushed", null))
            .Returns(Task.CompletedTask);

        _mockQueueService
            .Setup(q => q.SendToQueueAsync(It.IsAny<DTOs.NotificationResponseDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessNotificationAsync(template, channel, recipient, payload);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Template, Is.EqualTo(template));
        Assert.That(result.Channel, Is.EqualTo(channel));
        Assert.That(result.Recipient, Is.EqualTo(recipient));

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        _mockQueueService.Verify(q => q.SendToQueueAsync(It.IsAny<DTOs.NotificationResponseDto>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), "pushed", null), Times.Once);
    }

    [Test]
    public async Task ProcessNotificationAsync_ShouldNotPushToQueue_ForInApp()
    {
        // Arrange
        var template = "new-message";
        var channel = "in-app";
        var recipient = "user123";
        var payload = new { id = recipient, message = "Hello", from = "user456" };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessNotificationAsync(template, channel, recipient, payload);

        // Assert
        Assert.That(result, Is.Null);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        _mockQueueService.Verify(q => q.SendToQueueAsync(It.IsAny<DTOs.NotificationResponseDto>()), Times.Never);
        _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), "pushed", null), Times.Never);
    }
}