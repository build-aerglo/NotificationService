using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Repositories;

namespace NotificationService.Infrastructure.Services;

public class NotificationParamsService : INotificationParamsService
{
    private readonly INotificationParamsRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NotificationParamsService> _logger;
    private const string CacheKey = "NotificationParams";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public NotificationParamsService(
        INotificationParamsRepository repository,
        IMemoryCache cache,
        ILogger<NotificationParamsService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NotificationParams?> GetNotificationParamsAsync()
    {
        // Try to get from cache
        if (_cache.TryGetValue(CacheKey, out NotificationParams? cachedParams))
        {
            _logger.LogDebug("Retrieved notification params from cache");
            return cachedParams;
        }

        // Fetch from database
        _logger.LogDebug("Fetching notification params from database");
        var params_ = await _repository.GetNotificationParamsAsync();

        if (params_ != null)
        {
            // Cache the result
            _cache.Set(CacheKey, params_, CacheDuration);
            _logger.LogInformation("Cached notification params for {Duration} minutes", CacheDuration.TotalMinutes);
        }
        else
        {
            _logger.LogWarning("No notification params found in database");
        }

        return params_;
    }

    public void ClearCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("Notification params cache cleared");
    }
}
