using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NUnit.Framework;

namespace NotificationService.Api.Tests.Controllers;

[TestFixture]
public class SmsControllerTests
{
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<ILogger<SmsController>> _mockLogger = null!;
    private SmsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<SmsController>>();
        _controller = new SmsController(_mockNotificationService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task ForgetPassword_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var phone = "+1234567890";
        var code = "123456";
        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(),
            "forget-password",
            "sms",
            0,
            phone,
            new { phone, code },
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("forget-password", "sms", phone, It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ForgetPassword(phone, code);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "forget-password", "sms", phone, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task ForgetPassword_ShouldReturnBadRequest_WhenPhoneIsEmpty()
    {
        // Act
        var result = await _controller.ForgetPassword("", "123456");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Verification_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var phone = "+1234567890";
        var code = "654321";
        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(),
            "verification",
            "sms",
            0,
            phone,
            new { phone, code },
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("verification", "sms", phone, It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Verification(phone, code);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "verification", "sms", phone, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task OrderConfirmation_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var phone = "+1234567890";
        var orderId = "ORDER123";
        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(),
            "order-confirmation",
            "sms",
            0,
            phone,
            new { phone, orderId },
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("order-confirmation", "sms", phone, It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.OrderConfirmation(phone, orderId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "order-confirmation", "sms", phone, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task ForgetPassword_ShouldHandleError_AndReturn500()
    {
        // Arrange
        var phone = "+1234567890";
        var code = "123456";

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("forget-password", "sms", phone, It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ForgetPassword(phone, code);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }
}
