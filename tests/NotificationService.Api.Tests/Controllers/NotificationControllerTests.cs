using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NUnit.Framework;

namespace NotificationService.Api.Tests.Controllers;

[TestFixture]
public class NotificationControllerTests
{
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<ILogger<NotificationController>> _mockLogger = null!;
    private NotificationController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<NotificationController>>();
        _controller = new NotificationController(_mockNotificationService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task CreateNotification_ShouldReturnOk_WhenSuccessful()
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

        _mockNotificationService
            .Setup(s => s.AddAsync(dto))
            .ReturnsAsync(notification);

        // Act
        var result = await _controller.CreateNotification(dto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.AddAsync(dto), Times.Once);
    }

    [Test]
    public async Task GetNotification_ShouldReturnOk_WhenNotificationExists()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var notification = new Notification(
            "REVIEW_APPROVED",
            "PENDING",
            "Your review is live!",
            "Review approved",
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);

        // Act
        var result = await _controller.GetNotification(notificationId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetNotification_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        // Arrange
        var notificationId = Guid.NewGuid();

        _mockNotificationService
            .Setup(s => s.GetByIdAsync(notificationId))
            .ThrowsAsync(new NotificationNotFoundException("Notification not found"));

        // Act
        var result = await _controller.GetNotification(notificationId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task ReviewApproved_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new ReviewNotificationRequest(
            Email: "test@example.com",
            ReviewId: Guid.NewGuid()
        );

        var notification = new Notification(
            "REVIEW_APPROVED",
            "PENDING",
            "Great news! Your review has been published.",
            "Your review is live!",
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.AddAsync(It.IsAny<CreateNotificationDto>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _controller.ReviewApproved(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.AddAsync(
            It.Is<CreateNotificationDto>(dto => dto.NotificationType == "REVIEW_APPROVED")
        ), Times.Once);
    }

    [Test]
    public async Task ReviewRejected_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new ReviewRejectedNotificationRequest(
            Email: "test@example.com",
            ReviewId: Guid.NewGuid(),
            Reasons: new List<string> { "Review too short", "Contains spam" }
        );

        var notification = new Notification(
            "REVIEW_REJECTED",
            "PENDING",
            "Unfortunately, your review could not be published.",
            "Review could not be published",
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.AddAsync(It.IsAny<CreateNotificationDto>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _controller.ReviewRejected(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        _mockNotificationService.Verify(s => s.AddAsync(
            It.Is<CreateNotificationDto>(dto => 
                dto.NotificationType == "REVIEW_REJECTED" &&
                dto.MessageBody!.Contains("Review too short")
            )
        ), Times.Once);
    }

    [Test]
    public async Task ReviewFlagged_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new ReviewNotificationRequest(
            Email: "test@example.com",
            ReviewId: Guid.NewGuid()
        );

        var notification = new Notification(
            "REVIEW_FLAGGED",
            "PENDING",
            "Your review is being reviewed by our moderation team.",
            "Your review is under review",
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.AddAsync(It.IsAny<CreateNotificationDto>()))
            .ReturnsAsync(notification);

        // Act
        var result = await _controller.ReviewFlagged(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        _mockNotificationService.Verify(s => s.AddAsync(
            It.Is<CreateNotificationDto>(dto => dto.NotificationType == "REVIEW_FLAGGED")
        ), Times.Once);
    }

    [Test]
    public async Task ReviewApproved_ShouldHandleError_AndReturn500()
    {
        // Arrange
        var request = new ReviewNotificationRequest(
            Email: "test@example.com",
            ReviewId: Guid.NewGuid()
        );

        _mockNotificationService
            .Setup(s => s.AddAsync(It.IsAny<CreateNotificationDto>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ReviewApproved(request);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }
}