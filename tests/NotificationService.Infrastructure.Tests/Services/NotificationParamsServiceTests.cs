using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotificationService.Infrastructure.Services;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

namespace NotificationService.Infrastructure.Tests.Services;

[TestFixture]
public class NotificationParamsServiceTests
{
    private Mock<INotificationParamsRepository> _mockRepository = null!;
    private IMemoryCache _memoryCache = null!;
    private Mock<ILogger<NotificationParamsService>> _mockLogger = null!;
    private NotificationParamsService _service = null!;
    private NotificationParams _testParams = null!;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<INotificationParamsRepository>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<NotificationParamsService>>();

        _testParams = new NotificationParams
        {
            Id = 1,
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUser = "test@test.com",
            SmtpPassword = "password",
            EnableSsl = true,
            FromEmail = "noreply@test.com",
            FromName = "Test Service",
            SmsProvider = "Twilio",
            SmsAccountSid = "test_account_sid",
            SmsAuthToken = "test_auth_token",
            SmsFromNumber = "+1234567890",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _service = new NotificationParamsService(_mockRepository.Object, _memoryCache, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache?.Dispose();
    }

    [Test]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NotificationParamsService(null!, _memoryCache, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NotificationParamsService(_mockRepository.Object, null!, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NotificationParamsService(_mockRepository.Object, _memoryCache, null!));
    }

    [Test]
    public async Task GetNotificationParamsAsync_FirstCall_FetchesFromDatabase()
    {
        _mockRepository
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync(_testParams);

        var result = await _service.GetNotificationParamsAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SmtpHost, Is.EqualTo("smtp.test.com"));
        _mockRepository.Verify(x => x.GetNotificationParamsAsync(), Times.Once);
    }

    [Test]
    public async Task GetNotificationParamsAsync_SecondCall_ReturnsFromCache()
    {
        _mockRepository
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync(_testParams);

        // First call - should fetch from database
        var result1 = await _service.GetNotificationParamsAsync();

        // Second call - should return from cache
        var result2 = await _service.GetNotificationParamsAsync();

        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result1!.SmtpHost, Is.EqualTo(result2!.SmtpHost));

        // Repository should only be called once (first call)
        _mockRepository.Verify(x => x.GetNotificationParamsAsync(), Times.Once);
    }

    [Test]
    public async Task GetNotificationParamsAsync_WithNoParams_ReturnsNull()
    {
        _mockRepository
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync((NotificationParams?)null);

        var result = await _service.GetNotificationParamsAsync();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ClearCache_RemovesCachedParams()
    {
        _mockRepository
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync(_testParams);

        // First call - cache the params
        _service.GetNotificationParamsAsync().Wait();

        // Clear the cache
        _service.ClearCache();

        // Next call should fetch from database again
        _service.GetNotificationParamsAsync().Wait();

        // Repository should be called twice (before and after cache clear)
        _mockRepository.Verify(x => x.GetNotificationParamsAsync(), Times.Exactly(2));
    }

    [Test]
    public async Task GetNotificationParamsAsync_AfterCacheClear_FetchesFromDatabaseAgain()
    {
        _mockRepository
            .Setup(x => x.GetNotificationParamsAsync())
            .ReturnsAsync(_testParams);

        // First call
        await _service.GetNotificationParamsAsync();

        // Clear cache
        _service.ClearCache();

        // Second call after clearing cache
        await _service.GetNotificationParamsAsync();

        // Should be called twice
        _mockRepository.Verify(x => x.GetNotificationParamsAsync(), Times.Exactly(2));
    }
}
