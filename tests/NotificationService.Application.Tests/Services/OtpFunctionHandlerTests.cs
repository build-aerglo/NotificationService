using Microsoft.Extensions.Configuration;
using Moq;
using NotificationService.Application.Services;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;
using NUnit.Framework;

namespace NotificationService.Application.Tests.Services;

[TestFixture]
public class OtpFunctionHandlerTests
{
    private Mock<IBusinessVerificationRepository> _mockBusinessVerificationRepository = null!;
    private Mock<IPasswordResetRequestRepository> _mockPasswordResetRequestRepository = null!;
    private Mock<IConfiguration> _mockConfiguration = null!;
    private OtpFunctionHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _mockBusinessVerificationRepository = new Mock<IBusinessVerificationRepository>();
        _mockPasswordResetRequestRepository = new Mock<IPasswordResetRequestRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration
            .Setup(c => c.GetSection("PasswordResetSettings:ExpiryMinutes").Value)
            .Returns("60");

        _handler = new OtpFunctionHandler(
            _mockBusinessVerificationRepository.Object,
            _mockPasswordResetRequestRepository.Object,
            _mockConfiguration.Object);
    }

    [Test]
    [TestCase("emailVerification", true)]
    [TestCase("smsVerification", true)]
    [TestCase("resetPassword", true)]
    [TestCase("EMAILVERIFICATION", true)]
    [TestCase("invalidFunction", false)]
    [TestCase("", false)]
    public void IsValidFunction_ShouldReturnCorrectResult(string functionName, bool expected)
    {
        // Act
        var result = _handler.IsValidFunction(functionName);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task ExecuteAsync_EmailVerification_ShouldSetEmailVerified()
    {
        // Arrange
        var email = "test@example.com";
        var businessId = Guid.NewGuid();

        _mockBusinessVerificationRepository
            .Setup(r => r.GetBusinessIdByEmailAsync(email))
            .ReturnsAsync(businessId);

        _mockBusinessVerificationRepository
            .Setup(r => r.SetEmailVerifiedAsync(businessId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.ExecuteAsync("emailVerification", email);

        // Assert
        Assert.That(result, Is.True);
        _mockBusinessVerificationRepository.Verify(r => r.GetBusinessIdByEmailAsync(email), Times.Once);
        _mockBusinessVerificationRepository.Verify(r => r.SetEmailVerifiedAsync(businessId), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_EmailVerification_ShouldReturnFalse_WhenBusinessNotFound()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockBusinessVerificationRepository
            .Setup(r => r.GetBusinessIdByEmailAsync(email))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _handler.ExecuteAsync("emailVerification", email);

        // Assert
        Assert.That(result, Is.False);
        _mockBusinessVerificationRepository.Verify(r => r.SetEmailVerifiedAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_SmsVerification_ShouldSetPhoneVerified()
    {
        // Arrange
        var phone = "+1234567890";
        var businessId = Guid.NewGuid();

        _mockBusinessVerificationRepository
            .Setup(r => r.GetBusinessIdByPhoneAsync(phone))
            .ReturnsAsync(businessId);

        _mockBusinessVerificationRepository
            .Setup(r => r.SetPhoneVerifiedAsync(businessId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.ExecuteAsync("smsVerification", phone);

        // Assert
        Assert.That(result, Is.True);
        _mockBusinessVerificationRepository.Verify(r => r.GetBusinessIdByPhoneAsync(phone), Times.Once);
        _mockBusinessVerificationRepository.Verify(r => r.SetPhoneVerifiedAsync(businessId), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_SmsVerification_ShouldReturnFalse_WhenBusinessNotFound()
    {
        // Arrange
        var phone = "+0000000000";

        _mockBusinessVerificationRepository
            .Setup(r => r.GetBusinessIdByPhoneAsync(phone))
            .ReturnsAsync((Guid?)null);

        // Act
        var result = await _handler.ExecuteAsync("smsVerification", phone);

        // Assert
        Assert.That(result, Is.False);
        _mockBusinessVerificationRepository.Verify(r => r.SetPhoneVerifiedAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_ResetPassword_ShouldDeleteExistingAndCreateNew()
    {
        // Arrange
        var id = "test@example.com";

        _mockPasswordResetRequestRepository
            .Setup(r => r.DeleteByIdAsync(id))
            .Returns(Task.CompletedTask);

        _mockPasswordResetRequestRepository
            .Setup(r => r.AddAsync(It.IsAny<PasswordResetRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.ExecuteAsync("resetPassword", id);

        // Assert
        Assert.That(result, Is.True);
        _mockPasswordResetRequestRepository.Verify(r => r.DeleteByIdAsync(id), Times.Once);
        _mockPasswordResetRequestRepository.Verify(r => r.AddAsync(It.Is<PasswordResetRequest>(
            req => req.Id == id && req.ExpiresAt > DateTime.UtcNow)), Times.Once);
    }

    [Test]
    public void ExecuteAsync_ShouldThrowException_WhenFunctionIsUnknown()
    {
        // Arrange
        var id = "test@example.com";

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _handler.ExecuteAsync("unknownFunction", id));
    }
}
