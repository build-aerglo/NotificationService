using System.Text.Encodings.Web;
using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Services;

public class AzureQueueService : IQueueService
{
    private readonly QueueClient _queueClient;

    // UnsafeRelaxedJsonEscaping preserves characters like '+' and '/' as-is, which is
    // correct for queue messages consumed by trusted internal services.
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public AzureQueueService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureQueueStorage")
            ?? throw new ArgumentNullException(nameof(configuration), "AzureQueueStorage connection string is not configured.");

        _queueClient = new QueueClient(connectionString, "notifications");
        _queueClient.CreateIfNotExists();
    }

    public async Task SendToQueueAsync(NotificationResponseDto notification)
    {
        var message = JsonSerializer.Serialize(notification, SerializerOptions);
        await _queueClient.SendMessageAsync(message);
    }
}
