using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
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

    private static JsonElement CreatePayload(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    [Test]
    public async Task CreateNotification_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var payload = CreatePayload(new { code = "123456", title = "Password Reset" });
        var request = new CreateNotificationRequestDto("otp", "email", "test@example.com", payload);

        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(), "otp", "email", 0, "test@example.com",
            new { code = "123456", title = "Password Reset" }, DateTime.UtcNow);

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("otp", "email", "test@example.com", It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateNotification(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedResponse));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "otp", "email", "test@example.com", It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task CreateNotification_ShouldReturnBadRequest_WhenTemplateIsEmpty()
    {
        // Arrange
        var payload = CreatePayload(new { code = "123456" });
        var request = new CreateNotificationRequestDto("", "email", "test@example.com", payload);

        // Act
        var result = await _controller.CreateNotification(request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateNotification_ShouldReturnBadRequest_WhenChannelIsEmpty()
    {
        // Arrange
        var payload = CreatePayload(new { code = "123456" });
        var request = new CreateNotificationRequestDto("otp", "", "test@example.com", payload);

        // Act
        var result = await _controller.CreateNotification(request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateNotification_ShouldReturnBadRequest_WhenRecipientIsEmpty()
    {
        // Arrange
        var payload = CreatePayload(new { code = "123456" });
        var request = new CreateNotificationRequestDto("otp", "email", "", payload);

        // Act
        var result = await _controller.CreateNotification(request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateNotification_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        var payload = CreatePayload(new { code = "123456" });
        var request = new CreateNotificationRequestDto("otp", "email", "test@example.com", payload);

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateNotification(request);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task CreateNotification_ShouldReturnOk_WithSmsChannel()
    {
        // Arrange
        var payload = CreatePayload(new { phone = "+1234567890", code = "654321" });
        var request = new CreateNotificationRequestDto("forget-password", "sms", "+1234567890", payload);

        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(), "forget-password", "sms", 0, "+1234567890",
            new { phone = "+1234567890", code = "654321" }, DateTime.UtcNow);

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("forget-password", "sms", "+1234567890", It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateNotification(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task CreateNotification_ShouldReturnOkWithNull_WhenChannelIsInApp()
    {
        // Arrange
        var payload = CreatePayload(new { message = "Hello" });
        var request = new CreateNotificationRequestDto("new-message", "in-app", "user-123", payload);

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("new-message", "in-app", "user-123", It.IsAny<object>()))
            .ReturnsAsync((NotificationResponseDto?)null);

        // Act
        var result = await _controller.CreateNotification(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
    }
}
