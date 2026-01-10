using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotificationService.Api.Controllers;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Api.Tests.Controllers;

[TestFixture]
public class CommunicationControllerTests
{
    private Mock<IEmailService> _mockEmailService = null!;
    private Mock<ISmsService> _mockSmsService = null!;
    private Mock<ILogger<CommunicationController>> _mockLogger = null!;
    private CommunicationController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockSmsService = new Mock<ISmsService>();
        _mockLogger = new Mock<ILogger<CommunicationController>>();

        _controller = new CommunicationController(
            _mockEmailService.Object,
            _mockSmsService.Object,
            _mockLogger.Object);
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithNullEmailService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CommunicationController(null!, _mockSmsService.Object, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullSmsService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CommunicationController(_mockEmailService.Object, null!, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CommunicationController(_mockEmailService.Object, _mockSmsService.Object, null!));
    }

    #endregion

    #region SendForgetPasswordEmail Tests

    [Test]
    public async Task SendForgetPasswordEmail_WithValidRequest_ReturnsOk()
    {
        var request = new ForgetPasswordNotificationRequest("test@example.com", "123456");
        _mockEmailService.Setup(x => x.SendForgetPasswordEmailAsync(request.Email, request.Code))
            .ReturnsAsync(true);

        var result = await _controller.SendForgetPasswordEmail(request);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task SendForgetPasswordEmail_WithEmptyEmail_ReturnsBadRequest()
    {
        var request = new ForgetPasswordNotificationRequest("", "123456");

        var result = await _controller.SendForgetPasswordEmail(request);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SendForgetPasswordEmail_WithEmptyCode_ReturnsBadRequest()
    {
        var request = new ForgetPasswordNotificationRequest("test@example.com", "");

        var result = await _controller.SendForgetPasswordEmail(request);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SendForgetPasswordEmail_WhenServiceFails_ReturnsInternalServerError()
    {
        var request = new ForgetPasswordNotificationRequest("test@example.com", "123456");
        _mockEmailService.Setup(x => x.SendForgetPasswordEmailAsync(request.Email, request.Code))
            .ReturnsAsync(false);

        var result = await _controller.SendForgetPasswordEmail(request);

        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task SendForgetPasswordEmail_WhenExceptionThrown_ReturnsInternalServerError()
    {
        var request = new ForgetPasswordNotificationRequest("test@example.com", "123456");
        _mockEmailService.Setup(x => x.SendForgetPasswordEmailAsync(request.Email, request.Code))
            .ThrowsAsync(new Exception("Service error"));

        var result = await _controller.SendForgetPasswordEmail(request);

        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task SendForgetPasswordEmail_CallsServiceWithCorrectParameters()
    {
        var request = new ForgetPasswordNotificationRequest("test@example.com", "123456");
        _mockEmailService.Setup(x => x.SendForgetPasswordEmailAsync(request.Email, request.Code))
            .ReturnsAsync(true);

        await _controller.SendForgetPasswordEmail(request);

        _mockEmailService.Verify(
            x => x.SendForgetPasswordEmailAsync(request.Email, request.Code),
            Times.Once);
    }

    #endregion

    #region SendForgetPasswordSms Tests

    [Test]
    public async Task SendForgetPasswordSms_WithValidRequest_ReturnsOk()
    {
        var request = new ForgetPasswordSmsRequest("+1234567890", "123456");
        _mockSmsService.Setup(x => x.SendForgetPasswordSmsAsync(request.PhoneNumber, request.Code))
            .ReturnsAsync(true);

        var result = await _controller.SendForgetPasswordSms(request);

        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task SendForgetPasswordSms_WithEmptyPhoneNumber_ReturnsBadRequest()
    {
        var request = new ForgetPasswordSmsRequest("", "123456");

        var result = await _controller.SendForgetPasswordSms(request);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SendForgetPasswordSms_WithEmptyCode_ReturnsBadRequest()
    {
        var request = new ForgetPasswordSmsRequest("+1234567890", "");

        var result = await _controller.SendForgetPasswordSms(request);

        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SendForgetPasswordSms_WhenServiceFails_ReturnsInternalServerError()
    {
        var request = new ForgetPasswordSmsRequest("+1234567890", "123456");
        _mockSmsService.Setup(x => x.SendForgetPasswordSmsAsync(request.PhoneNumber, request.Code))
            .ReturnsAsync(false);

        var result = await _controller.SendForgetPasswordSms(request);

        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task SendForgetPasswordSms_WhenExceptionThrown_ReturnsInternalServerError()
    {
        var request = new ForgetPasswordSmsRequest("+1234567890", "123456");
        _mockSmsService.Setup(x => x.SendForgetPasswordSmsAsync(request.PhoneNumber, request.Code))
            .ThrowsAsync(new Exception("Service error"));

        var result = await _controller.SendForgetPasswordSms(request);

        var statusCodeResult = result as ObjectResult;
        Assert.That(statusCodeResult, Is.Not.Null);
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(500));
    }

    [Test]
    public async Task SendForgetPasswordSms_CallsServiceWithCorrectParameters()
    {
        var request = new ForgetPasswordSmsRequest("+1234567890", "123456");
        _mockSmsService.Setup(x => x.SendForgetPasswordSmsAsync(request.PhoneNumber, request.Code))
            .ReturnsAsync(true);

        await _controller.SendForgetPasswordSms(request);

        _mockSmsService.Verify(
            x => x.SendForgetPasswordSmsAsync(request.PhoneNumber, request.Code),
            Times.Once);
    }

    #endregion
}
