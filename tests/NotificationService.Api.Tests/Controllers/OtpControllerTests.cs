using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Api.Controllers;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Exceptions;
using NUnit.Framework;

namespace NotificationService.Api.Tests.Controllers;

[TestFixture]
public class OtpControllerTests
{
    private Mock<IOtpService> _mockOtpService = null!;
    private Mock<ILogger<OtpController>> _mockLogger = null!;
    private OtpController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockOtpService = new Mock<IOtpService>();
        _mockLogger = new Mock<ILogger<OtpController>>();
        _controller = new OtpController(_mockOtpService.Object, _mockLogger.Object);
    }

    #region CreateOtp Tests

    [Test]
    public async Task CreateOtp_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var id = "test@example.com";
        var type = "email";
        var expectedResponse = new OtpResponseDto(
            id,
            "123456",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(60)
        );

        _mockOtpService
            .Setup(s => s.CreateOtpAsync(It.IsAny<CreateOtpRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateOtp(id, type);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockOtpService.Verify(s => s.CreateOtpAsync(It.Is<CreateOtpRequestDto>(
            r => r.Id == id && r.Type == type)), Times.Once);
    }

    [Test]
    public async Task CreateOtp_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Act
        var result = await _controller.CreateOtp("", "email");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateOtp_ShouldReturnBadRequest_WhenTypeIsEmpty()
    {
        // Act
        var result = await _controller.CreateOtp("test@example.com", "");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateOtp_ShouldReturnBadRequest_WhenTypeIsInvalid()
    {
        // Act
        var result = await _controller.CreateOtp("test@example.com", "invalid");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateOtp_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        _mockOtpService
            .Setup(s => s.CreateOtpAsync(It.IsAny<CreateOtpRequestDto>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateOtp("test@example.com", "email");

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region ValidateOtp Tests

    [Test]
    public async Task ValidateOtp_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var id = "test@example.com";
        var code = "123456";
        var type = "emailVerification";
        var expectedResponse = new ValidateOtpResponseDto(true, "OTP validated successfully.");

        _mockOtpService
            .Setup(s => s.ValidateOtpAsync(It.IsAny<ValidateOtpRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ValidateOtp(id, code, type);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockOtpService.Verify(s => s.ValidateOtpAsync(It.Is<ValidateOtpRequestDto>(
            r => r.Id == id && r.Code == code && r.Type == type)), Times.Once);
    }

    [Test]
    public async Task ValidateOtp_ShouldReturnBadRequest_WhenIdIsEmpty()
    {
        // Act
        var result = await _controller.ValidateOtp("", "123456", "emailVerification");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ValidateOtp_ShouldReturnBadRequest_WhenCodeIsEmpty()
    {
        // Act
        var result = await _controller.ValidateOtp("test@example.com", "", "emailVerification");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ValidateOtp_ShouldReturnBadRequest_WhenTypeIsEmpty()
    {
        // Act
        var result = await _controller.ValidateOtp("test@example.com", "123456", "");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ValidateOtp_ShouldReturnNotFound_WhenOtpNotFound()
    {
        // Arrange
        _mockOtpService
            .Setup(s => s.ValidateOtpAsync(It.IsAny<ValidateOtpRequestDto>()))
            .ThrowsAsync(new OtpNotFoundException());

        // Act
        var result = await _controller.ValidateOtp("test@example.com", "123456", "emailVerification");

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task ValidateOtp_ShouldReturnBadRequest_WhenOtpExpired()
    {
        // Arrange
        _mockOtpService
            .Setup(s => s.ValidateOtpAsync(It.IsAny<ValidateOtpRequestDto>()))
            .ThrowsAsync(new OtpExpiredException());

        // Act
        var result = await _controller.ValidateOtp("test@example.com", "123456", "emailVerification");

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ValidateOtp_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        _mockOtpService
            .Setup(s => s.ValidateOtpAsync(It.IsAny<ValidateOtpRequestDto>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.ValidateOtp("test@example.com", "123456", "emailVerification");

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion

    #region DeleteManyOtp Tests

    [Test]
    public async Task DeleteManyOtp_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var ids = new[] { "id1@example.com", "id2@example.com" };
        var request = new DeleteManyOtpRequestDto(ids);

        _mockOtpService
            .Setup(s => s.DeleteManyOtpAsync(request))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteManyOtp(request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));

        _mockOtpService.Verify(s => s.DeleteManyOtpAsync(request), Times.Once);
    }

    [Test]
    public async Task DeleteManyOtp_ShouldReturnBadRequest_WhenIdsIsNull()
    {
        // Arrange
        var request = new DeleteManyOtpRequestDto(null!);

        // Act
        var result = await _controller.DeleteManyOtp(request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task DeleteManyOtp_ShouldReturnBadRequest_WhenIdsIsEmpty()
    {
        // Arrange
        var request = new DeleteManyOtpRequestDto(Array.Empty<string>());

        // Act
        var result = await _controller.DeleteManyOtp(request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task DeleteManyOtp_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        var ids = new[] { "id1@example.com" };
        var request = new DeleteManyOtpRequestDto(ids);

        _mockOtpService
            .Setup(s => s.DeleteManyOtpAsync(request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteManyOtp(request);

        // Assert
        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    #endregion
}
