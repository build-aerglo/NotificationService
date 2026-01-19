using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using NotificationService.Application.Interfaces;
using NotificationService.Application.DTOs;
using System.Text.Json;

namespace NotificationService.Infrastructure.Services;

public class AzureQueueService : IQueueService
{
    private readonly QueueClient _queueClient;

    public AzureQueueService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureQueueStorage")!;
        _queueClient = new QueueClient(connectionString, "notifications");
        _queueClient.CreateIfNotExists();
    }

    public async Task SendToQueueAsync(NotificationResponseDto notification)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    
        var message = JsonSerializer.Serialize(notification, options);
        await _queueClient.SendMessageAsync(message);
    }

}
