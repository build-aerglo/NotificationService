using Moq;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Domain.Repositories;
using NUnit.Framework;

namespace NotificationService.Application.Tests.Services;

[TestFixture]
public class NotificationServiceTests
{
    private Mock<INotificationRepository> _mockRepository = null!;
    private Application.Services.NotificationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<INotificationRepository>();
        _service = new Application.Services.NotificationService(_mockRepository.Object);
    }

    // ✅ ESSENTIAL: Create notification successfully
    [Test]
    public async Task AddAsync_ValidNotification_ShouldReturnNotification()
    {
        // Arrange
        var dto = new CreateNotificationDto(
            NotificationType: "REVIEW_APPROVED",
            NotificationDate: DateTime.UtcNow,
            NotificationStatus: "PENDING",
            MessageHeader: "Your review is live!",
            MessageBody: "Great news! Your review has been published."
        );

        var notification = new Notification(
            dto.NotificationType,
            dto.NotificationStatus,
            dto.MessageBody,
            dto.MessageHeader,
            dto.NotificationDate
        );

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NotificationType, Is.EqualTo("REVIEW_APPROVED"));
        Assert.That(result.MessageHeader, Is.EqualTo("Your review is live!"));
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
    }

    // ✅ ESSENTIAL: Get notification by ID
    [Test]
    public async Task GetByIdAsync_ExistingNotification_ShouldReturnNotification()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = new Notification(
            "REVIEW_REJECTED",
            "PENDING",
            "Your review could not be published.",
            "Review rejected",
            DateTime.UtcNow
        );

        _mockRepository
            .Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        var result = await _service.GetByIdAsync(notificationId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.NotificationType, Is.EqualTo("REVIEW_REJECTED"));
    }

    // ✅ ESSENTIAL: Get notification throws when not found
    [Test]
    public void GetByIdAsync_NonExistingNotification_ShouldThrowException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotificationNotFoundException>(
            async () => await _service.GetByIdAsync(notificationId)
        );

        Assert.That(ex!.Message, Does.Contain("Notification not found"));
    }

    // ✅ ESSENTIAL: Review approved notification
    [Test]
    public async Task AddAsync_ReviewApprovedNotification_ShouldSucceed()
    {
        // Arrange
        var dto = new CreateNotificationDto(
            NotificationType: "REVIEW_APPROVED",
            NotificationDate: DateTime.UtcNow,
            NotificationStatus: "PENDING",
            MessageHeader: "Your review is live!",
            MessageBody: "Great news! Your review has been published and is now visible to others."
        );

        var notification = new Notification(
            dto.NotificationType,
            dto.NotificationStatus,
            dto.MessageBody,
            dto.MessageHeader,
            dto.NotificationDate
        );

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.NotificationType, Is.EqualTo("REVIEW_APPROVED"));
        Assert.That(result.MessageBody, Does.Contain("published"));
    }

    // ✅ ESSENTIAL: Review rejected notification
    [Test]
    public async Task AddAsync_ReviewRejectedNotification_ShouldSucceed()
    {
        // Arrange
        var dto = new CreateNotificationDto(
            NotificationType: "REVIEW_REJECTED",
            NotificationDate: DateTime.UtcNow,
            NotificationStatus: "PENDING",
            MessageHeader: "Review could not be published",
            MessageBody: "Unfortunately, your review could not be published for the following reasons: Review too short."
        );

        var notification = new Notification(
            dto.NotificationType,
            dto.NotificationStatus,
            dto.MessageBody,
            dto.MessageHeader,
            dto.NotificationDate
        );

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.NotificationType, Is.EqualTo("REVIEW_REJECTED"));
        Assert.That(result.MessageBody, Does.Contain("could not be published"));
    }

    // ✅ ESSENTIAL: Review flagged notification
    [Test]
    public async Task AddAsync_ReviewFlaggedNotification_ShouldSucceed()
    {
        // Arrange
        var dto = new CreateNotificationDto(
            NotificationType: "REVIEW_FLAGGED",
            NotificationDate: DateTime.UtcNow,
            NotificationStatus: "PENDING",
            MessageHeader: "Your review is under review",
            MessageBody: "Your review is being reviewed by our moderation team."
        );

        var notification = new Notification(
            dto.NotificationType,
            dto.NotificationStatus,
            dto.MessageBody,
            dto.MessageHeader,
            dto.NotificationDate
        );

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _service.AddAsync(dto);

        // Assert
        Assert.That(result.NotificationType, Is.EqualTo("REVIEW_FLAGGED"));
        Assert.That(result.MessageBody, Does.Contain("moderation team"));
    }
}