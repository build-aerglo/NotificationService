using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NUnit.Framework;

namespace NotificationService.Api.Tests.Controllers;

[TestFixture]
public class EmailControllerTests
{
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<ILogger<EmailController>> _mockLogger = null!;
    private EmailController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<EmailController>>();
        _controller = new EmailController(_mockNotificationService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task ForgetPassword_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";
        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(),
            "forget-password",
            "email",
            0,
            email,
            new { email, code },
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("forget-password", "email", email, It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ForgetPassword(email, code);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "forget-password", "email", email, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task ForgetPassword_ShouldReturnBadRequest_WhenEmailIsEmpty()
    {
        // Act
        var result = await _controller.ForgetPassword("", "123456");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Welcome_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(),
            "welcome",
            "email",
            0,
            email,
            new { email, firstName },
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("welcome", "email", email, It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Welcome(email, firstName);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "welcome", "email", email, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task Verification_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var email = "test@example.com";
        var code = "654321";
        var expectedResponse = new NotificationResponseDto(
            Guid.NewGuid(),
            "verification",
            "email",
            0,
            email,
            new { email, code },
            DateTime.UtcNow
        );

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("verification", "email", email, It.IsAny<object>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Verification(email, code);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockNotificationService.Verify(s => s.ProcessNotificationAsync(
            "verification", "email", email, It.IsAny<object>()), Times.Once);
    }

    [Test]
    public async Task ForgetPassword_ShouldHandleError_AndReturn500()
    {
        // Arrange
        var email = "test@example.com";
        var code = "123456";

        _mockNotificationService
            .Setup(s => s.ProcessNotificationAsync("forget-password", "email", email, It.IsAny<object>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ForgetPassword(email, code);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }
}
