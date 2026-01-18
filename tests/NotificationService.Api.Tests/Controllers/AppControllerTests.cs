using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.Interfaces;
using NUnit.Framework;

namespace NotificationService.Api.Tests.Controllers;

[TestFixture]
public class AppControllerTests
{
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<ILogger<AppController>> _mockLogger = null!;
    private AppController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<AppController>>();
        _controller = new AppController(_mockNotificationService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task NewMessage_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var id = "user123";
        var message = "Hello world";
        var from = "user456";

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("new-message", "in-app", id, It.IsAny<object>()))
            .ReturnsAsync((NotificationService.Application.DTOs.NotificationResponseDto?)null);

        // Act
        var result = await _controller.NewMessage(id, message, from);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "new-message", "in-app", id, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task NewMessage_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Act
        var result = await _controller.NewMessage("", "Hello", "user456");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ReviewApproved_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var id = "user123";
        var reviewId = "review456";

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("review-approved", "in-app", id, It.IsAny<object>()))
            .ReturnsAsync((NotificationService.Application.DTOs.NotificationResponseDto?)null);

        // Act
        var result = await _controller.ReviewApproved(id, reviewId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "review-approved", "in-app", id, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task ReviewRejected_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var id = "user123";
        var reviewId = "review456";
        var reason = "Inappropriate content";

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("review-rejected", "in-app", id, It.IsAny<object>()))
            .ReturnsAsync((NotificationService.Application.DTOs.NotificationResponseDto?)null);

        // Act
        var result = await _controller.ReviewRejected(id, reviewId, reason);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "review-rejected", "in-app", id, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task NewMessage_ShouldHandleError_AndReturn500()
    {
        // Arrange
        var id = "user123";
        var message = "Hello world";
        var from = "user456";

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("new-message", "in-app", id, It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.NewMessage(id, message, from);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }
}
