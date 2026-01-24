using Microsoft.Extensions.Configuration;
using Moq;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Exceptions;
using NotificationService.Domain.Repositories;
using NUnit.Framework;

namespace NotificationService.Application.Tests.Services;

[TestFixture]
public class OtpServiceTests
{
    private Mock<IOtpRepository> _mockOtpRepository = null!;
    private Mock<IOtpFunctionHandler> _mockOtpFunctionHandler = null!;
    private Mock<IConfiguration> _mockConfiguration = null!;
    private OtpService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockOtpRepository = new Mock<IOtpRepository>();
        _mockOtpFunctionHandler = new Mock<IOtpFunctionHandler>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration to return 60 minutes for OTP expiry
        _mockConfiguration
            .Setup(c => c.GetSection("OtpSettings:ExpiryMinutes").Value)
            .Returns("60");

        _service = new OtpService(
            _mockOtpRepository.Object,
            _mockOtpFunctionHandler.Object,
            _mockConfiguration.Object);
    }

    [Test]
    public async Task CreateOtpAsync_ShouldDeleteExistingOtps_AndCreateNew()
    {
        // Arrange
        var request = new CreateOtpRequestDto("test@example.com", "email");

        _mockOtpRepository
            .Setup(r => r.DeleteByIdAsync(request.Id))
            .Returns(Task.CompletedTask);

        _mockOtpRepository
            .Setup(r => r.AddAsync(It.IsAny<Otp>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateOtpAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(request.Id));
        Assert.That(result.Code, Has.Length.EqualTo(6));
        Assert.That(result.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));

        _mockOtpRepository.Verify(r => r.DeleteByIdAsync(request.Id), Times.Once);
        _mockOtpRepository.Verify(r => r.AddAsync(It.IsAny<Otp>()), Times.Once);
    }

    [Test]
    public async Task CreateOtpAsync_ShouldGenerateSixDigitCode()
    {
        // Arrange
        var request = new CreateOtpRequestDto("+1234567890", "sms");

        _mockOtpRepository
            .Setup(r => r.DeleteByIdAsync(request.Id))
            .Returns(Task.CompletedTask);

        _mockOtpRepository
            .Setup(r => r.AddAsync(It.IsAny<Otp>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateOtpAsync(request);

        // Assert
        Assert.That(result.Code, Has.Length.EqualTo(6));
        Assert.That(int.TryParse(result.Code, out var code), Is.True);
        Assert.That(code, Is.GreaterThanOrEqualTo(100000));
        Assert.That(code, Is.LessThan(1000000));
    }

    [Test]
    public async Task ValidateOtpAsync_ShouldReturnSuccess_WhenOtpIsValid()
    {
        // Arrange
        var request = new ValidateOtpRequestDto("test@example.com", "123456", "emailVerification");
        var otp = new Otp("test@example.com", "123456", DateTime.UtcNow.AddMinutes(60));

        _mockOtpFunctionHandler
            .Setup(h => h.IsValidFunction(request.Type))
            .Returns(true);

        _mockOtpRepository
            .Setup(r => r.GetByIdAndCodeAsync(request.Id, request.Code))
            .ReturnsAsync(otp);

        _mockOtpRepository
            .Setup(r => r.DeleteByIdAndCodeAsync(request.Id, request.Code))
            .Returns(Task.CompletedTask);

        _mockOtpFunctionHandler
            .Setup(h => h.ExecuteAsync(request.Type, request.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ValidateOtpAsync(request);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Is.EqualTo("OTP validated successfully."));

        _mockOtpRepository.Verify(r => r.DeleteByIdAndCodeAsync(request.Id, request.Code), Times.Once);
        _mockOtpFunctionHandler.Verify(h => h.ExecuteAsync(request.Type, request.Id), Times.Once);
    }

    [Test]
    public void ValidateOtpAsync_ShouldThrowOtpNotFoundException_WhenOtpDoesNotExist()
    {
        // Arrange
        var request = new ValidateOtpRequestDto("test@example.com", "123456", "emailVerification");

        _mockOtpFunctionHandler
            .Setup(h => h.IsValidFunction(request.Type))
            .Returns(true);

        _mockOtpRepository
            .Setup(r => r.GetByIdAndCodeAsync(request.Id, request.Code))
            .ReturnsAsync((Otp?)null);

        // Act & Assert
        Assert.ThrowsAsync<OtpNotFoundException>(async () =>
            await _service.ValidateOtpAsync(request));
    }

    [Test]
    public void ValidateOtpAsync_ShouldThrowOtpExpiredException_WhenOtpIsExpired()
    {
        // Arrange
        var request = new ValidateOtpRequestDto("test@example.com", "123456", "emailVerification");
        var expiredOtp = new Otp("test@example.com", "123456", DateTime.UtcNow.AddMinutes(-10));

        _mockOtpFunctionHandler
            .Setup(h => h.IsValidFunction(request.Type))
            .Returns(true);

        _mockOtpRepository
            .Setup(r => r.GetByIdAndCodeAsync(request.Id, request.Code))
            .ReturnsAsync(expiredOtp);

        _mockOtpRepository
            .Setup(r => r.DeleteByIdAndCodeAsync(request.Id, request.Code))
            .Returns(Task.CompletedTask);

        // Act & Assert
        Assert.ThrowsAsync<OtpExpiredException>(async () =>
            await _service.ValidateOtpAsync(request));

        _mockOtpRepository.Verify(r => r.DeleteByIdAndCodeAsync(request.Id, request.Code), Times.Once);
    }

    [Test]
    public async Task ValidateOtpAsync_ShouldReturnFailure_WhenFunctionTypeIsInvalid()
    {
        // Arrange
        var request = new ValidateOtpRequestDto("test@example.com", "123456", "invalidFunction");

        _mockOtpFunctionHandler
            .Setup(h => h.IsValidFunction(request.Type))
            .Returns(false);

        // Act
        var result = await _service.ValidateOtpAsync(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid OTP function type"));
    }

    [Test]
    public async Task ValidateOtpAsync_ShouldReturnFailure_WhenFunctionExecutionFails()
    {
        // Arrange
        var request = new ValidateOtpRequestDto("test@example.com", "123456", "emailVerification");
        var otp = new Otp("test@example.com", "123456", DateTime.UtcNow.AddMinutes(60));

        _mockOtpFunctionHandler
            .Setup(h => h.IsValidFunction(request.Type))
            .Returns(true);

        _mockOtpRepository
            .Setup(r => r.GetByIdAndCodeAsync(request.Id, request.Code))
            .ReturnsAsync(otp);

        _mockOtpRepository
            .Setup(r => r.DeleteByIdAndCodeAsync(request.Id, request.Code))
            .Returns(Task.CompletedTask);

        _mockOtpFunctionHandler
            .Setup(h => h.ExecuteAsync(request.Type, request.Id))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateOtpAsync(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Failed to execute OTP function."));
    }

    [Test]
    public async Task DeleteManyOtpAsync_ShouldCallRepository()
    {
        // Arrange
        var ids = new[] { "id1@example.com", "id2@example.com", "+1234567890" };
        var request = new DeleteManyOtpRequestDto(ids);

        _mockOtpRepository
            .Setup(r => r.DeleteManyByIdsAsync(request.Ids))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteManyOtpAsync(request);

        // Assert
        _mockOtpRepository.Verify(r => r.DeleteManyByIdsAsync(request.Ids), Times.Once);
    }
}
